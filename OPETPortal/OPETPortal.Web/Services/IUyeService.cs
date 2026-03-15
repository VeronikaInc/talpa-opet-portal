using OPETPortal.Web.Models;

namespace OPETPortal.Web.Services;

public interface IUyeService
{
    // Kullanıcı sorgulama
    Task<UyeKod?> GetByTcknAsync(string tckn);
    Task LogSorguAsync(string tckn, string sonuc, string? ipAdresi = null);
    Task KodGoruntulendiAsync(string tckn);

    // Admin — arama
    Task<UyeKod?> GetByKodAsync(string opetKodu);
    Task<IEnumerable<UyeKod>> GetAllAsync(string? search);

    // Admin — dashboard
    Task<DashboardStats> GetDashboardStatsAsync();

    // Admin — loglar
    Task<List<SorguLog>> GetSorguLoglarAsync(string? tckn = null, string? sonuc = null,
                                              DateTime? baslangic = null, DateTime? bitis = null,
                                              int skip = 0, int take = 20);
    Task<int> GetSorguLogCountAsync(string? tckn = null, string? sonuc = null,
                                    DateTime? baslangic = null, DateTime? bitis = null);

    // Admin — yönetim
    Task<bool> UyeEkleAsync(UyeKod uye);
    Task UpdateBorcluAsync(string tckn, bool borclu);
}
