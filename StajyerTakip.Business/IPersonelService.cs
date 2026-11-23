using StajyerTakip.DataAccess;
using StajyerTakip.Entities;

namespace StajyerTakip.Business;

public interface IPersonelService
{
    Task<IReadOnlyList<PersonelDTO>> TumunuGetirAsync(CancellationToken cancellationToken = default);
}

public sealed class PersonelService(IPersonelRepository repository) : IPersonelService
{
    public Task<IReadOnlyList<PersonelDTO>> TumunuGetirAsync(CancellationToken cancellationToken = default)
        => repository.TumunuGetirAsync(cancellationToken);
}
