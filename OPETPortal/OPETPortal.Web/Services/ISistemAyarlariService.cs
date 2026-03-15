namespace OPETPortal.Web.Services;

public interface ISistemAyarlariService
{
    Task<bool> BasvuruAktifMiAsync();
    Task<int> GetMaxKodLimitiAsync();
    Task<int> GetDagitimSayisiAsync();
    Task SetAyarAsync(string anahtar, string deger);
}
