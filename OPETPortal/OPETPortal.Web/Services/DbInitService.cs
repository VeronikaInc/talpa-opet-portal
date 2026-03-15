using Dapper;
using Npgsql;

namespace OPETPortal.Web.Services;

public class DbInitService
{
    private readonly string _connectionString;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DbInitService> _logger;

    public DbInitService(IConfiguration configuration, IWebHostEnvironment env, ILogger<DbInitService> logger)
    {
        _connectionString = configuration.GetValue<string>("CONNECTION_STRING")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string not configured.");
        _env = env;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        using var conn = new NpgsqlConnection(_connectionString);

        // 001 — Base table
        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS uye_kodlar (
                tckn        VARCHAR(11) PRIMARY KEY,
                opet_kodu   VARCHAR(50) NOT NULL,
                borclu      BOOLEAN DEFAULT FALSE,
                created_at  TIMESTAMP DEFAULT NOW(),
                updated_at  TIMESTAMP DEFAULT NOW()
            );");

        // 003 — Admin revamp columns & sorgu_log
        await conn.ExecuteAsync(@"
            ALTER TABLE uye_kodlar
                ADD COLUMN IF NOT EXISTS kod_goruntulendi   BOOLEAN   DEFAULT FALSE,
                ADD COLUMN IF NOT EXISTS goruntuleme_tarihi TIMESTAMP NULL,
                ADD COLUMN IF NOT EXISTS test_kaydi         BOOLEAN   DEFAULT FALSE;

            CREATE TABLE IF NOT EXISTS sorgu_log (
                id            SERIAL PRIMARY KEY,
                tckn          VARCHAR(11)  NOT NULL,
                sorgu_tarihi  TIMESTAMP    DEFAULT NOW(),
                sonuc         VARCHAR(20)  NOT NULL CHECK (sonuc IN ('basarili', 'borclu', 'bulunamadi')),
                ip_adresi     VARCHAR(45)  NULL
            );

            CREATE INDEX IF NOT EXISTS idx_sorgu_log_tckn         ON sorgu_log (tckn);
            CREATE INDEX IF NOT EXISTS idx_sorgu_log_sorgu_tarihi ON sorgu_log (sorgu_tarihi DESC);
        ");

        // 004 — Sistem ayarları
        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS sistem_ayarlari (
                anahtar    VARCHAR(50) PRIMARY KEY,
                deger      TEXT        NOT NULL,
                guncelleme TIMESTAMP   DEFAULT NOW()
            );

            INSERT INTO sistem_ayarlari (anahtar, deger) VALUES
                ('basvuru_aktif',  'true'),
                ('max_kod_limiti', '0')
            ON CONFLICT (anahtar) DO NOTHING;
        ");

        // Seed data — run only if table is empty
        await RunSeedIfEmptyAsync(conn);
    }

    private async Task RunSeedIfEmptyAsync(NpgsqlConnection conn)
    {
        var count = await conn.QuerySingleAsync<int>("SELECT COUNT(*) FROM uye_kodlar");
        if (count > 0)
        {
            _logger.LogInformation("DB already seeded ({Count} rows). Skipping seed.", count);
            return;
        }

        // Look for seed file embedded in app or in migrations/ folder
        var seedPath = Path.Combine(_env.ContentRootPath, "migrations", "seed_data.sql");
        if (!File.Exists(seedPath))
        {
            _logger.LogWarning("Seed file not found at {Path}. Skipping seed.", seedPath);
            return;
        }

        _logger.LogInformation("Running seed from {Path}...", seedPath);
        var sql = await File.ReadAllTextAsync(seedPath);
        await conn.ExecuteAsync(sql);
        var seeded = await conn.QuerySingleAsync<int>("SELECT COUNT(*) FROM uye_kodlar");
        _logger.LogInformation("Seed complete. {Count} rows inserted.", seeded);
    }
}
