namespace OPETPortal.Web.Models;

public class UyeKod
{
    public string Tckn { get; set; } = string.Empty;
    public string OpetKodu { get; set; } = string.Empty;
    public bool Borclu { get; set; }
    public bool KodGoruntulendi { get; set; }
    public DateTime? GoruntulmeTarihi { get; set; }
    public bool TestKaydi { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
