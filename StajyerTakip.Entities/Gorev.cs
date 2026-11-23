namespace StajyerTakip.Entities;

public sealed record Gorev
{
    public int Id { get; init; }
    public int PersonelId { get; init; }
    public string Baslik { get; init; } = "";
    public string? Aciklama { get; init; }
    public GorevDurumu Durum { get; init; }
    public DateTime OlusturmaTarihi { get; init; }
    public DateTime? BitisTarihi { get; init; }
    public byte[] RowVersion { get; init; } = [];
}
