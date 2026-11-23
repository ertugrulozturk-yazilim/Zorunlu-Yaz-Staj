using System.Buffers.Binary;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StajyerTakip.DataAccess;
using StajyerTakip.Entities;

namespace StajyerTakip.Tests;

// Yalnızca test projesindedir. Gerçek API her zaman SQL repository'lerini kullanır.
public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    public TestStore Store { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IGorevRepository>();
            services.RemoveAll<IPersonelRepository>();
            services.AddSingleton<IGorevRepository>(Store);
            services.AddSingleton<IPersonelRepository>(Store);
        });
    }
}

public sealed class TestStore : IGorevRepository, IPersonelRepository
{
    private readonly Dictionary<int, (Gorev Gorev, bool Silindi)> _rows = [];
    private readonly object _gate = new();
    private int _nextId;
    private long _version;
    public bool HataOlustur { get; set; }
    public IReadOnlyList<PersonelDTO> Personeller { get; } = [new(7, "Demo Personel", "Yazılım Stajyeri", "Yazılım"), new(42, "İkinci Personel", "Yazılım Geliştirici", "Yazılım")];

    public bool SilindiMi(int id) { lock (_gate) return _rows.TryGetValue(id, out var row) && row.Silindi; }
    private byte[] YeniSurum() { var bytes = new byte[8]; BinaryPrimitives.WriteInt64BigEndian(bytes, ++_version); return bytes; }

    Task<IReadOnlyList<Gorev>> IGorevRepository.TumunuGetirAsync(CancellationToken cancellationToken)
    {
        if (HataOlustur) throw new InvalidOperationException("SAKLI_BAGLANTI_AYRINTISI");
        lock (_gate) return Task.FromResult<IReadOnlyList<Gorev>>(_rows.Values.Where(x => !x.Silindi).Select(x => x.Gorev).OrderByDescending(x => x.Id).ToArray());
    }

    public Task<Gorev?> GetirAsync(int id, CancellationToken cancellationToken)
    {
        lock (_gate) return Task.FromResult(_rows.TryGetValue(id, out var row) && !row.Silindi ? row.Gorev : null);
    }

    public Task<Gorev> EkleAsync(Gorev gorev, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var row = gorev with { Id = ++_nextId, RowVersion = YeniSurum(), OlusturmaTarihi = DateTime.UtcNow };
            _rows[row.Id] = (row, false);
            return Task.FromResult(row);
        }
    }

    public Task<Gorev?> DurumGuncelleAsync(int id, GorevDurumu durum, byte[] rowVersion, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (!_rows.TryGetValue(id, out var row) || row.Silindi || !row.Gorev.RowVersion.SequenceEqual(rowVersion)) return Task.FromResult<Gorev?>(null);
            var updated = row.Gorev with { Durum = durum, RowVersion = YeniSurum(), BitisTarihi = durum == GorevDurumu.Tamamlandi ? row.Gorev.BitisTarihi ?? DateTime.UtcNow : null };
            _rows[id] = (updated, false);
            return Task.FromResult<Gorev?>(updated);
        }
    }

    public Task<bool> SilAsync(int id, byte[] rowVersion, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (!_rows.TryGetValue(id, out var row) || row.Silindi || !row.Gorev.RowVersion.SequenceEqual(rowVersion)) return Task.FromResult(false);
            _rows[id] = (row.Gorev with { RowVersion = YeniSurum() }, true);
            return Task.FromResult(true);
        }
    }

    Task<IReadOnlyList<PersonelDTO>> IPersonelRepository.TumunuGetirAsync(CancellationToken cancellationToken) => Task.FromResult(Personeller);
    public Task<bool> VarMiAsync(int id, CancellationToken cancellationToken) => Task.FromResult(Personeller.Any(p => p.Id == id));
}
