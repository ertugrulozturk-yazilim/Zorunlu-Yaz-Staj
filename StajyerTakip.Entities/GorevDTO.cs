using System.ComponentModel.DataAnnotations;

namespace StajyerTakip.Entities;

public sealed record GorevDTO(int Id, int PersonelId, string Baslik, string? Aciklama,
    GorevDurumu Durum, DateTime OlusturmaTarihi, DateTime? BitisTarihi, string RowVersion);

public sealed class GorevOlusturDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Bir personel seçin.")]
    public int PersonelId { get; set; }

    [Required(ErrorMessage = "Görev başlığı zorunludur.")]
    [StringLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
    public string Baslik { get; set; } = "";

    [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    public string? Aciklama { get; set; }
}

public sealed class GorevDurumGuncelleDTO
{
    // Boş JSON nesnesi yanlışlıkla Yapılacak durumuna dönüşmemelidir.
    [Required(ErrorMessage = "Yeni durum zorunludur.")]
    public GorevDurumu? Durum { get; set; }
}

public sealed record PersonelDTO(int Id, string AdSoyad, string PozisyonAdi, string DepartmanAdi);
