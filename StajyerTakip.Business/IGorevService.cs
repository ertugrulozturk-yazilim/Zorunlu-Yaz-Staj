using StajyerTakip.Entities;

namespace StajyerTakip.Business;

public interface IGorevService
{
    Task<IReadOnlyList<GorevDTO>> TumunuGetirAsync(CancellationToken cancellationToken = default);
    Task<GorevDTO> GetirAsync(int id, CancellationToken cancellationToken = default);
    Task<GorevDTO> EkleAsync(GorevOlusturDTO dto, CancellationToken cancellationToken = default);
    Task<GorevDTO> DurumGuncelleAsync(int id, GorevDurumu durum, byte[] rowVersion, CancellationToken cancellationToken = default);
    Task SilAsync(int id, byte[] rowVersion, CancellationToken cancellationToken = default);
}
