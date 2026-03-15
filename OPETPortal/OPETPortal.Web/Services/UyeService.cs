using Dapper;
using Npgsql;
using OPETPortal.Web.Models;

namespace OPETPortal.Web.Services;

public class UyeService : IUyeService
{
    private readonly string _connectionString;

    public UyeService(IConfiguration configuration)
    {
        _connectionString = configuration.GetValue<string>("CONNECTION_STRING")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string not configured.");
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    // ── Kullanıcı sorgulama ──────────────────────────────────────────────────

    public async Task<UyeKod?> GetByTcknAsync(string tckn)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<UyeKod>(@"
            SELECT tckn, opet_kodu AS OpetKodu, borclu AS Borclu,
                   kod_goruntulendi AS KodGoruntulendi,
                   goruntuleme_tarihi AS GoruntulmeTarihi,
                   test_kaydi AS TestKaydi,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM uye_kodlar WHERE tckn = @tckn",
            new { tckn });
    }

    public async Task LogSorguAsync(string tckn, string sonuc, string? ipAdresi = null)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO sorgu_log (tckn, sonuc, ip_adresi, sorgu_tarihi)
            VALUES (@tckn, @sonuc, @ipAdresi, NOW())",
            new { tckn, sonuc, ipAdresi });
    }

    public async Task KodGoruntulendiAsync(string tckn)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(@"
            UPDATE uye_kodlar
            SET kod_goruntulendi   = TRUE,
                goruntuleme_tarihi = NOW(),
                updated_at         = NOW()
            WHERE tckn = @tckn AND kod_goruntulendi = FALSE",
            new { tckn });
    }

    // ── Admin — arama ────────────────────────────────────────────────────────

    public async Task<UyeKod?> GetByKodAsync(string opetKodu)
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<UyeKod>(@"
            SELECT tckn, opet_kodu AS OpetKodu, borclu AS Borclu,
                   kod_goruntulendi AS KodGoruntulendi,
                   goruntuleme_tarihi AS GoruntulmeTarihi,
                   test_kaydi AS TestKaydi,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM uye_kodlar WHERE opet_kodu ILIKE @opetKodu",
            new { opetKodu });
    }

    public async Task<IEnumerable<UyeKod>> GetAllAsync(string? search)
    {
        using var conn = CreateConnection();
        if (string.IsNullOrWhiteSpace(search))
        {
            return await conn.QueryAsync<UyeKod>(@"
                SELECT tckn, opet_kodu AS OpetKodu, borclu AS Borclu,
                       kod_goruntulendi AS KodGoruntulendi,
                       goruntuleme_tarihi AS GoruntulmeTarihi,
                       test_kaydi AS TestKaydi,
                       created_at AS CreatedAt, updated_at AS UpdatedAt
                FROM uye_kodlar ORDER BY created_at DESC LIMIT 500");
        }
        return await conn.QueryAsync<UyeKod>(@"
            SELECT tckn, opet_kodu AS OpetKodu, borclu AS Borclu,
                   kod_goruntulendi AS KodGoruntulendi,
                   goruntuleme_tarihi AS GoruntulmeTarihi,
                   test_kaydi AS TestKaydi,
                   created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM uye_kodlar
            WHERE tckn ILIKE @s OR opet_kodu ILIKE @s
            ORDER BY created_at DESC LIMIT 200",
            new { s = $"%{search}%" });
    }

    // ── Admin — dashboard ────────────────────────────────────────────────────

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleAsync<DashboardStats>(@"
            SELECT
                COUNT(*)                                        AS ToplamUye,
                COUNT(*) FILTER (WHERE kod_goruntulendi = TRUE) AS KodGoruntuleyenCount,
                COUNT(*) FILTER (WHERE borclu = TRUE)           AS BorcluCount,
                COUNT(*) FILTER (WHERE kod_goruntulendi = FALSE AND borclu = FALSE) AS AktifKodCount
            FROM uye_kodlar");
    }

    // ── Admin — loglar ───────────────────────────────────────────────────────

    public async Task<List<SorguLog>> GetSorguLoglarAsync(
        string? tckn = null, string? sonuc = null,
        DateTime? baslangic = null, DateTime? bitis = null,
        int skip = 0, int take = 20)
    {
        using var conn = CreateConnection();
        var (where, p) = BuildLogWhere(tckn, sonuc, baslangic, bitis);
        p.Add("skip", skip); p.Add("take", take);
        var rows = await conn.QueryAsync<SorguLog>($@"
            SELECT id AS Id, tckn AS Tckn, sorgu_tarihi AS SorguTarihi,
                   sonuc AS Sonuc, ip_adresi AS IpAdresi
            FROM sorgu_log
            {where}
            ORDER BY sorgu_tarihi DESC
            OFFSET @skip LIMIT @take", p);
        return rows.ToList();
    }

    public async Task<int> GetSorguLogCountAsync(
        string? tckn = null, string? sonuc = null,
        DateTime? baslangic = null, DateTime? bitis = null)
    {
        using var conn = CreateConnection();
        var (where, p) = BuildLogWhere(tckn, sonuc, baslangic, bitis);
        return await conn.QuerySingleAsync<int>($"SELECT COUNT(*) FROM sorgu_log {where}", p);
    }

    private static (string where, DynamicParameters p) BuildLogWhere(
        string? tckn, string? sonuc, DateTime? baslangic, DateTime? bitis)
    {
        var conditions = new List<string>();
        var p = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(tckn))      { conditions.Add("tckn = @tckn");               p.Add("tckn", tckn); }
        if (!string.IsNullOrWhiteSpace(sonuc))      { conditions.Add("sonuc = @sonuc");              p.Add("sonuc", sonuc); }
        if (baslangic.HasValue)                     { conditions.Add("sorgu_tarihi >= @baslangic");  p.Add("baslangic", baslangic); }
        if (bitis.HasValue)                         { conditions.Add("sorgu_tarihi <= @bitis");      p.Add("bitis", bitis); }
        var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
        return (where, p);
    }

    // ── Admin — yönetim ──────────────────────────────────────────────────────

    public async Task<bool> UyeEkleAsync(UyeKod uye)
    {
        using var conn = CreateConnection();
        var affected = await conn.ExecuteAsync(@"
            INSERT INTO uye_kodlar (tckn, opet_kodu, borclu, test_kaydi, created_at, updated_at)
            VALUES (@Tckn, @OpetKodu, @Borclu, @TestKaydi, NOW(), NOW())
            ON CONFLICT (tckn) DO NOTHING",
            uye);
        return affected > 0;
    }

    public async Task UpdateBorcluAsync(string tckn, bool borclu)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE uye_kodlar SET borclu = @borclu, updated_at = NOW() WHERE tckn = @tckn",
            new { tckn, borclu });
    }
}
