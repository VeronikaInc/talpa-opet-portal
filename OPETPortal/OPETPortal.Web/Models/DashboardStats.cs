namespace OPETPortal.Web.Models;

public class DashboardStats
{
    public int ToplamUye { get; set; }
    public int KodGoruntuleyenCount { get; set; }
    public int BorcluCount { get; set; }
    public int AktifKodCount { get; set; } // kod_goruntulendi = false
}
