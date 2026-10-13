namespace StajyerTakip.Entities
{
    public class GorevDTO
    {
        public int Id { get; set; }

        public int PersonelId { get; set; }

        public string Baslik { get; set; } = "";

        public string? Aciklama { get; set; }

        public string Durum { get; set; } = "";
    }
}