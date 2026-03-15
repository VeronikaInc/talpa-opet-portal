namespace OPETPortal.Web.Models;

public class SorguLog
{
    public int Id { get; set; }
    public string Tckn { get; set; } = string.Empty;
    public DateTime SorguTarihi { get; set; }
    public string Sonuc { get; set; } = string.Empty; // basarili | borclu | bulunamadi
    public string? IpAdresi { get; set; }
}
