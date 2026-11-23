using StajyerTakip.DataAccess;
using StajyerTakip.Entities;

namespace StajyerTakip.Business;

public sealed class GorevService(IGorevRepository gorevRepository, IPersonelRepository personelRepository) : IGorevService
{
    public async Task<IReadOnlyList<GorevDTO>> TumunuGetirAsync(CancellationToken cancellationToken = default)
        => (await gorevRepository.TumunuGetirAsync(cancellationToken)).Select(DtoyaCevir).ToArray();

    public async Task<GorevDTO> GetirAsync(int id, CancellationToken cancellationToken = default)
        => DtoyaCevir(await gorevRepository.GetirAsync(id, cancellationToken)
            ?? throw new IslemHatasi(IslemHataTuru.Bulunamadi, "Görev bulunamadı."));

    public async Task<GorevDTO> EkleAsync(GorevOlusturDTO dto, CancellationToken cancellationToken = default)
    {
        var baslik = dto.Baslik?.Trim() ?? "";
        var aciklama = string.IsNullOrWhiteSpace(dto.Aciklama) ? null : dto.Aciklama.Trim();
        if (baslik.Length is < 1 or > 150)
            throw new IslemHatasi(IslemHataTuru.GecersizVeri, "Başlık 1 ile 150 karakter arasında olmalıdır.");
        if (aciklama?.Length > 2000)
            throw new IslemHatasi(IslemHataTuru.GecersizVeri, "Açıklama en fazla 2000 karakter olabilir.");
        if (dto.PersonelId < 1 || !await personelRepository.VarMiAsync(dto.PersonelId, cancellationToken))
            throw new IslemHatasi(IslemHataTuru.GecersizVeri, "Seçilen personel bulunamadı. Personel listesini yenileyin.");

        var gorev = new Gorev { PersonelId = dto.PersonelId, Baslik = baslik, Aciklama = aciklama, Durum = GorevDurumu.Yapilacak };
        return DtoyaCevir(await gorevRepository.EkleAsync(gorev, cancellationToken));
    }

    public async Task<GorevDTO> DurumGuncelleAsync(int id, GorevDurumu durum, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        SurumuDogrula(rowVersion);
        if (!Enum.IsDefined(durum)) throw new IslemHatasi(IslemHataTuru.GecersizVeri, "Geçersiz görev durumu.");
        var sonuc = await gorevRepository.DurumGuncelleAsync(id, durum, rowVersion, cancellationToken);
        if (sonuc is null) await BulunamamaVeyaSurumHatasiAsync(id, cancellationToken);
        return DtoyaCevir(sonuc!);
    }

    public async Task SilAsync(int id, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        SurumuDogrula(rowVersion);
        if (!await gorevRepository.SilAsync(id, rowVersion, cancellationToken))
            await BulunamamaVeyaSurumHatasiAsync(id, cancellationToken);
    }

    private async Task BulunamamaVeyaSurumHatasiAsync(int id, CancellationToken cancellationToken)
    {
        if (await gorevRepository.GetirAsync(id, cancellationToken) is null)
            throw new IslemHatasi(IslemHataTuru.Bulunamadi, "Görev bulunamadı veya silinmiş.");
        throw new IslemHatasi(IslemHataTuru.SurumUyusmazligi,
            "Bu görev başka bir işlemde değiştirildi. Listeyi yenileyip güncel kayıtla tekrar deneyin.");
    }

    private static void SurumuDogrula(byte[] rowVersion)
    {
        if (rowVersion.Length != 8) throw new IslemHatasi(IslemHataTuru.GecersizVeri, "Geçersiz kayıt sürümü.");
    }

    private static GorevDTO DtoyaCevir(Gorev g) => new(g.Id, g.PersonelId, g.Baslik, g.Aciklama, g.Durum,
        g.OlusturmaTarihi, g.BitisTarihi, Convert.ToBase64String(g.RowVersion));
}
