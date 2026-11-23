using StajyerTakip.Entities;

namespace StajyerTakip.DataAccess;

public interface IGorevRepository
{
    Task<IReadOnlyList<Gorev>> TumunuGetirAsync(CancellationToken cancellationToken);
    Task<Gorev?> GetirAsync(int id, CancellationToken cancellationToken);
    Task<Gorev> EkleAsync(Gorev gorev, CancellationToken cancellationToken);
    Task<Gorev?> DurumGuncelleAsync(int id, GorevDurumu durum, byte[] rowVersion, CancellationToken cancellationToken);
    Task<bool> SilAsync(int id, byte[] rowVersion, CancellationToken cancellationToken);
}

public interface IPersonelRepository
{
    Task<IReadOnlyList<PersonelDTO>> TumunuGetirAsync(CancellationToken cancellationToken);
    Task<bool> VarMiAsync(int id, CancellationToken cancellationToken);
}
