# Stajyer Görev Takip Sistemi

Ertuğrul Öztürk'ün staj projesinin sunum için düzenlenmiş sürümü. Görev ekleme, listeleme, durum değiştirme ve soft delete işlemlerini Azure SQL üzerinde gerçekleştirir.

**İlk açılış için `ILK_ONCE_OKU.md` dosyasını takip et.** Panel ve API aynı ASP.NET Core uygulamasından sunulur: `http://localhost:5237`.

## Kullanılan teknolojiler

C#, .NET 10, ASP.NET Core Web API, ADO.NET, Microsoft.Data.SqlClient, SQL Server / Azure SQL, HTML, CSS ve JavaScript. Arayüzde dış CDN veya JavaScript paketi gerekmiyor. Veri erişimi ADO.NET ile yapılır; bu sürüm Entity Framework veya başka bir ORM kullanmaz.

## Projenin yapısı

| Proje veya klasör | Sorumluluk |
| --- | --- |
| StajyerTakip.API | HTTP istekleri, JSON dönüştürme, hata cevapları, DI ve wwwroot arayüzü |
| StajyerTakip.Business | Başlık/personel/durum kontrolleri ve görev iş akışları |
| StajyerTakip.DataAccess | Repository sözleşmeleri, SQL sorguları, bağlantı oluşturma |
| StajyerTakip.Entities | Görev modeli, DTO'lar ve GorevDurumu enum'u |
| StajyerTakip.Tests | xUnit ve gerçek ASP.NET test sunucusu ile API testleri |
| Veritabani | Boş veya mevcut veritabanı için elle çalıştırılabilecek SQL betikleri |

Controller servisi çağırır; servis repository arayüzü üzerinden veri erişimini ister. Somut SQL repository'leri Program.cs içinde dependency injection ile bağlanır. Veritabanı işlemleri async/await ve CancellationToken kullanır.

## Sunum için yapılan düzenlemeler

- SQL sorguları Business katmanından DataAccess repository'lerine taşındı.
- GorevDurumu enum'u eklendi; JSON ve SQL'de mevcut Türkçe durum değerleri korunuyor.
- Oluşturma ve güncelleme istekleri için ayrı DTO'lar eklendi.
- Güncelleme ve silmede RowVersion ve If-Match ile eski sürüm kontrolü tamamlandı.
- Sabit personel numarası kaldırıldı, veritabanından gelen personel seçimi eklendi.
- Boş/uzun başlık, uzun açıklama, olmayan personel ve tanımsız durumlar doğrulanıyor.
- SQL parametrelerinin türleri ve uzunlukları açıkça belirtiliyor.
- Kayıt silme IsDeleted üzerinden devam ediyor. Tamamlanmada BitisTarihi atanıyor; görev yeniden açıldığında temizleniyor.
- Hatalar ProblemDetails yapısında dönüyor; veritabanı arızası geçersiz veri gibi 400'e dönüştürülmüyor.
- Arayüz aynı origin üzerinde sunuluyor; ayrı Live Server ve geniş CORS izni kaldırıldı.
- Kullanıcı metinleri textContent ile çiziliyor; HTML olarak eklenmiyor.
- Gönderim sırasında tekrar kaydetme engelleniyor; istekler 20 saniye sonunda kullanıcıya açıklama gösteriyor.
- Arama, durum filtresi ve görev sayıları eklendi.
- Eski bağlantı parolası ve derleme artıkları pakete konulmadı.

## Çalıştırma ve test

```powershell
dotnet restore StajyerTakipSistemi.sln
dotnet build StajyerTakipSistemi.sln -c Release
dotnet test StajyerTakipSistemi.sln -c Release
dotnet run --project StajyerTakip.API/StajyerTakip.API.csproj --launch-profile http
```

Testler kullanıcı Azure hesabına bağlanmaz; API hattını ve iş kurallarını test repository'leriyle doğrular. Uygulamanın normal çalışmasında gerçek SQL repository'leri kullanılır. Test ve üretim veri yolları karışmaz. Test kapsamının sınırları `DOGRULAMA_RAPORU.md` içinde açıklanır.

## HTTP davranışı

Görev oluşturma 201, başarılı okuma/güncelleme 200, başarılı silme 204 döndürür. Geçersiz veri 400, olmayan görev 404, eski kayıt sürümü 412, eksik If-Match 428 ve veritabanı erişim hataları 503 ile açıklanır. Ayrıntılar `API_DOKUMANTASYONU.md` dosyasında.

## Güvenlik ve kapsam

SQL bağlantısında TLS şifrelemesi zorunludur ve sunucu sertifikası doğrulanır. Bu, hazır güvenli bağlantı mekanizmasının kullanılmasıdır; uygulamaya özel bir şifreleme algoritması geliştirildiği anlamına gelmez. Kullanıcı girişi ve rol yetkilendirmesi bu MVP'nin kapsamında değildir. İnternete yayımlanacak bir üründe kimlik doğrulama, yetkilendirme, HTTPS ve işletim ayarları ayrıca tamamlanmalıdır.

API'nin yayında olduğu veya özel şifreleme/ORM geliştirildiği iddia edilmemelidir. Son sürümün doğru anlatımı ve staj defterindeki teknik ifadeler için `SUNUM_NOTLARI.md` dosyasına bak.

## Bağımlılık notu

Microsoft.Data.SqlClient 7.0.2 korunmuştur. Transitif PKCS paketinin bilinen güvenlik sorunlarını gidermek için System.Security.Cryptography.Pkcs 10.0.13 doğrudan sabitlenmiştir. Paket güvenlik uyarıları kapatılmamıştır. Güncel paket taramasının sonucu doğrulama raporunda yer alır.

## İsteğe bağlı arayüz testleri

API testleri için Node.js gerekmez. DOM davranış testlerini yeniden çalıştırmak istersen Node.js 22.12 veya üzeriyle:

```powershell
cd ArayuzTestleri
npm install
npm test
```

Bu testler taklit API yanıtları kullanır ve gerçek tarayıcı görsel testi yerine geçmez.
