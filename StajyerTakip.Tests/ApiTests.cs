using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using StajyerTakip.DataAccess;
using Xunit;

namespace StajyerTakip.Tests;

public sealed class ApiTests : IDisposable
{
    private readonly TestApplicationFactory _factory = new();
    private readonly HttpClient _client;
    public ApiTests() => _client = _factory.CreateClient();
    public void Dispose() { _client.Dispose(); _factory.Dispose(); }

    [Fact]
    public async Task Arayuz_Ve_Statik_Dosyalar_Api_Ile_Ayni_Hosttan_Sunulur()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Görev Yönetim Paneli", await response.Content.ReadAsStringAsync());
        Assert.Contains("script-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/app.js")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/styles.css")).StatusCode);
    }

    [Fact]
    public async Task Eksik_Baglanti_Ayari_503_Ile_Aciklanir()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment("Testing").ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = "" })));
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/Gorev");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("User Secrets", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Ekle_Listele_Guncelle_Sil_Akisi()
    {
        var response = await _client.PostAsJsonAsync("/api/Gorev", new { personelId = 7, baslik = "  API testi  ", aciklama = "Açıklama" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await Oku(response);
        Assert.Equal("API testi", task.GetProperty("baslik").GetString());
        Assert.Equal("Yapılacak", task.GetProperty("durum").GetString());
        var id = task.GetProperty("id").GetInt32();
        var version = task.GetProperty("rowVersion").GetString()!;
        Assert.Equal($"\"{version}\"", response.Headers.ETag?.Tag);
        Assert.EndsWith($"/api/Gorev/{id}", response.Headers.Location?.ToString());
        var list = await _client.GetFromJsonAsync<JsonElement>("/api/Gorev");
        Assert.Single(list.EnumerateArray());
        var update = await Guncelle(id, version, "Tamamlandı");
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var completed = await Oku(update);
        Assert.Equal("Tamamlandı", completed.GetProperty("durum").GetString());
        Assert.NotEqual(JsonValueKind.Null, completed.GetProperty("bitisTarihi").ValueKind);
        Assert.NotEqual(version, completed.GetProperty("rowVersion").GetString());
        var deleted = await Sil(id, completed.GetProperty("rowVersion").GetString()!);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.True(_factory.Store.SilindiMi(id));
        Assert.Empty((await _client.GetFromJsonAsync<JsonElement>("/api/Gorev")).EnumerateArray());
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/Gorev/{id}")).StatusCode);
    }

    [Theory]
    [InlineData(0, "Görev")]
    [InlineData(999, "Görev")]
    [InlineData(7, "")]
    [InlineData(7, "   ")]
    public async Task Gecersiz_Personel_Ve_Baslik_Kaydedilmez(int personelId, string baslik)
    {
        var response = await _client.PostAsJsonAsync("/api/Gorev", new { personelId, baslik });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty((await _client.GetFromJsonAsync<JsonElement>("/api/Gorev")).EnumerateArray());
    }

    [Theory]
    [InlineData(151, 0)]
    [InlineData(1, 2001)]
    public async Task Uzun_Metinler_Reddedilir(int baslikUzunlugu, int aciklamaUzunlugu)
    {
        var response = await _client.PostAsJsonAsync("/api/Gorev", new { personelId = 7, baslik = new string('a', baslikUzunlugu), aciklama = new string('b', aciklamaUzunlugu) });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("Beklemede")]
    [InlineData("tamamlandı")]
    public async Task Tanimlanmamis_Durum_Reddedilir(string durum)
    {
        var task = await Ekle();
        var response = await Guncelle(task.GetProperty("id").GetInt32(), task.GetProperty("rowVersion").GetString()!, durum);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Eksik_Durum_Kaydi_Sifirlamaz()
    {
        var task = await Ekle();
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/Gorev/{task.GetProperty("id").GetInt32()}") { Content = JsonContent.Create(new { }) };
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{task.GetProperty("rowVersion").GetString()}\"");
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.SendAsync(request)).StatusCode);
    }

    [Theory]
    [InlineData(null, 428)]
    [InlineData("yanlis", 400)]
    [InlineData("\"AQ==\"", 400)]
    [InlineData("W/\"AAAAAAAAAAE=\"", 400)]
    public async Task Surum_Olmadan_Degisiklik_Yapilmaz(string? ifMatch, int expected)
    {
        var task = await Ekle();
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/Gorev/{task.GetProperty("id").GetInt32()}") { Content = JsonContent.Create(new { durum = "Tamamlandı" }) };
        if (ifMatch is not null) request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        Assert.Equal(expected, (int)(await _client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task Iki_Istemcide_Eski_Surum_Guncellemesi_Ve_Silmesi_Engellenir()
    {
        var task = await Ekle();
        var id = task.GetProperty("id").GetInt32();
        var old = task.GetProperty("rowVersion").GetString()!;
        Assert.Equal(HttpStatusCode.OK, (await Guncelle(id, old, "Devam Ediyor")).StatusCode);
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await Guncelle(id, old, "Tamamlandı")).StatusCode);
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await Sil(id, old)).StatusCode);
        var current = await _client.GetFromJsonAsync<JsonElement>($"/api/Gorev/{id}");
        Assert.Equal("Devam Ediyor", current.GetProperty("durum").GetString());
    }

    [Fact]
    public async Task Tamamlanan_Gorev_Yeniden_Acilinca_Bitis_Tarihi_Temizlenir()
    {
        var task = await Ekle(); var id = task.GetProperty("id").GetInt32();
        var done = await Oku(await Guncelle(id, task.GetProperty("rowVersion").GetString()!, "Tamamlandı"));
        var reopened = await Oku(await Guncelle(id, done.GetProperty("rowVersion").GetString()!, "Devam Ediyor"));
        Assert.Equal(JsonValueKind.Null, reopened.GetProperty("bitisTarihi").ValueKind);
    }

    [Fact]
    public async Task Olmayan_Gorev_Icin_404_Doner()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/Gorev/999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Guncelle(999, "AAAAAAAAAAE=", "Tamamlandı")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Sil(999, "AAAAAAAAAAE=")).StatusCode);
    }

    [Fact]
    public async Task Personel_Secimi_Bir_Numarali_Kayda_Bagli_Degildir()
    {
        var people = await _client.GetFromJsonAsync<JsonElement>("/api/Personel");
        Assert.Equal(2, people.GetArrayLength());
        var response = await _client.PostAsJsonAsync("/api/Gorev", new { personelId = 42, baslik = "İkinci personele görev" });
        Assert.Equal(42, (await Oku(response)).GetProperty("personelId").GetInt32());
    }

    [Fact]
    public async Task HTML_Ve_SQL_Benzeri_Girdi_Veri_Olarak_Korunur()
    {
        const string title = "<img src=x onerror=alert(1)> ' OR 1=1 --";
        var response = await _client.PostAsJsonAsync("/api/Gorev", new { personelId = 7, baslik = title });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(title, (await Oku(response)).GetProperty("baslik").GetString());
    }

    [Fact]
    public async Task Beklenmeyen_Hata_Istemciye_Ic_Ayrintilari_Sizdirmaz()
    {
        _factory.Store.HataOlustur = true;
        var response = await _client.GetAsync("/api/Gorev");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("SAKLI_BAGLANTI_AYRINTISI", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public void Sql_Baglantisi_TLS_Ve_Sertifika_Dogrulamasini_Zorunlu_Tutar()
    {
        using var connection = new SqlConnectionFactory("Server=localhost;Database=Test;Encrypt=False;TrustServerCertificate=True;").Olustur();
        var settings = new SqlConnectionStringBuilder(connection.ConnectionString);
        Assert.Equal(SqlConnectionEncryptOption.Mandatory, settings.Encrypt);
        Assert.False(settings.TrustServerCertificate);
    }

    private async Task<JsonElement> Ekle() => await Oku(await _client.PostAsJsonAsync("/api/Gorev", new { personelId = 7, baslik = "Örnek görev" }));
    private static async Task<JsonElement> Oku(HttpResponseMessage response) => (await response.Content.ReadFromJsonAsync<JsonElement>()).Clone();
    private async Task<HttpResponseMessage> Guncelle(int id, string version, string durum)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/Gorev/{id}") { Content = JsonContent.Create(new { durum }) };
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version}\"");
        return await _client.SendAsync(request);
    }
    private async Task<HttpResponseMessage> Sil(int id, string version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Gorev/{id}");
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version}\"");
        return await _client.SendAsync(request);
    }
}
