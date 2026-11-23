namespace StajyerTakip.Entities;

public enum GorevDurumu { Yapilacak = 0, DevamEdiyor = 1, Tamamlandi = 2 }

public static class GorevDurumuMetinleri
{
    // API ve mevcut Azure tablosu aynı Türkçe durum değerlerini kullanır.
    public static string MetneCevir(this GorevDurumu durum) => durum switch
    {
        GorevDurumu.Yapilacak => "Yapılacak",
        GorevDurumu.DevamEdiyor => "Devam Ediyor",
        GorevDurumu.Tamamlandi => "Tamamlandı",
        _ => throw new ArgumentOutOfRangeException(nameof(durum))
    };

    public static bool TryParse(string? metin, out GorevDurumu durum)
    {
        switch (metin)
        {
            case "Yapılacak": durum = GorevDurumu.Yapilacak; return true;
            case "Devam Ediyor": durum = GorevDurumu.DevamEdiyor; return true;
            case "Tamamlandı": durum = GorevDurumu.Tamamlandi; return true;
            default: durum = default; return false;
        }
    }
}
