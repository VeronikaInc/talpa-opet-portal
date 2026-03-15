using Dapper;
using Npgsql;

namespace OPETPortal.Web.Services;

public class SistemAyarlariService : ISistemAyarlariService
{
    private readonly string _connectionString;

    public SistemAyarlariService(IConfiguration configuration)
    {
        _connectionString = configuration.GetValue<string>("CONNECTION_STRING")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string not configured.");
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<bool> BasvuruAktifMiAsync()
    {
        using var conn = CreateConnection();
        var deger = await conn.QuerySingleOrDefaultAsync<string>(
            "SELECT deger FROM sistem_ayarlari WHERE anahtar = 'basvuru_aktif'");
        return deger?.ToLowerInvariant() == "true";
    }

    public async Task<int> GetMaxKodLimitiAsync()
    {
        using var conn = CreateConnection();
        var deger = await conn.QuerySingleOrDefaultAsync<string>(
            "SELECT deger FROM sistem_ayarlari WHERE anahtar = 'max_kod_limiti'");
        return int.TryParse(deger, out var limit) ? limit : 0;
    }

    public async Task<int> GetDagitimSayisiAsync()
    {
        using var conn = CreateConnection();
        return await conn.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM uye_kodlar WHERE kod_goruntulendi = TRUE");
    }

    public async Task SetAyarAsync(string anahtar, string deger)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO sistem_ayarlari (anahtar, deger, guncelleme)
            VALUES (@anahtar, @deger, NOW())
            ON CONFLICT (anahtar) DO UPDATE
                SET deger = @deger, guncelleme = NOW()",
            new { anahtar, deger });
    }
}
