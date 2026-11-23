using System.Text.Json;
using System.Text.Json.Serialization;
using StajyerTakip.Entities;

namespace StajyerTakip.API.Serialization;

public sealed class GorevDurumuJsonConverter : JsonConverter<GorevDurumu>
{
    public override GorevDurumu Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && GorevDurumuMetinleri.TryParse(reader.GetString(), out var durum)) return durum;
        throw new JsonException("Durum, Yapılacak, Devam Ediyor veya Tamamlandı olmalıdır.");
    }
    public override void Write(Utf8JsonWriter writer, GorevDurumu value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.MetneCevir());
}
