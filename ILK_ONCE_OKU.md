# Sunum sürümünü çalıştırma

Bu paket .NET 10 ile hazırlanmış kaynak projedir. Azure veritabanındaki mevcut görevlerin silinmesi veya yeniden oluşturulması gerekmez. Parola pakete konulmadı; çalışan bağlantını bir kez kendi bilgisayarında ayarlayacaksın.

## 1 Projeyi aç

ZIP'i eski projenin üzerine yazmadan yeni bir klasöre çıkar. Eski API çalışıyorsa durdur; iki uygulama aynı 5237 portunu kullanmasın. Visual Studio'da `StajyerTakipSistemi.sln` dosyasını aç. .NET 10 SDK ve ASP.NET web geliştirme bileşeni yüklü olmalı.

## 2 Çalışan Azure bağlantını ekle

Çözüm Gezgini'nde **StajyerTakip.API** projesine sağ tıkla ve **Kullanıcı Gizli Dizilerini Yönet / Manage User Secrets** seçeneğini aç.

Açılan `secrets.json` dosyasına aşağıdaki yapıyı yaz. Yer tutucu metnin tamamını, bilgisayarında şu anda çalışan eski projenin `DefaultConnection` değeriyle değiştir:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "CALISAN_AZURE_BAGLANTI_DIZENI_BURAYA_YAPISTIR"
  }
}
```

Bağlantı dizesinin tamamını kullan: sunucu, veritabanı, kullanıcı ve güncel SQL parolası birlikte bulunmalı. Gönderdiğin ilk ZIP'teki eski parolayı kullanmak zorunda değilsin; sonradan düzelttiğin ve çalışan değeri al. Parolanı paylaşma. Bu dosya proje klasörünün dışında kalır ve teslim ZIP'ine girmez. User Secrets geliştirme ayarıdır; bir şifreleme kasası değildir.

Bu paket `stajyer-takip-sunum-ertugrul-ozturk` UserSecretsId değerini kullanır. `appsettings.json` içindeki boş değer Development profilinde User Secrets tarafından geçersiz kılınır. Önceden tanımlı `ConnectionStrings__DefaultConnection` ortam değişkeni varsa o daha önceliklidir; eski bir değer bırakma.

## 3 Uygulamayı çalıştır

`StajyerTakip.API` projesini başlangıç projesi yap, üstte **http** profilini seç ve F5'e bas. Açılmazsa tarayıcıda **http://localhost:5237** adresine git.

Arayüz de API de bu adrestedir. Eski `index.html` veya Live Server üzerinden açma. `/todos` adresi kullanılmıyor. `/api/Gorev` JSON listesidir; panel ana adreste açılır.

Alternatif olarak proje klasöründeki `BASLAT.bat` dosyasını çalıştır veya terminalde:

```powershell
dotnet run --project StajyerTakip.API/StajyerTakip.API.csproj --launch-profile http
```

Terminali açık tut. HTTPS ile göstermek istersen Visual Studio'daki **https** profili `https://localhost:7237` adresini kullanır; yerel geliştirme sertifikasının bilgisayarında güvenilir olması gerekir.

## 4 Mevcut veritabanını kullan

Gönderdiğin şemada gerekli `RowVersion`, `IsDeleted`, `OlusturmaTarihi` ve `BitisTarihi` sütunları zaten var. Önce mevcut veritabanınla çalıştırmayı dene. Uygulama açılışta tablo veya örnek kayıt oluşturmaz.

Eksik tablo/sütun hatası varsa SSMS'te doğru Azure veritabanını seçip `Veritabani/01_Kurulum_ve_Guncelleme.sql` dosyasını inceleyerek çalıştır. Betik veritabanını veya tabloları düşürmez, mevcut görevleri silmez. Yalnızca eksik yapıları ekler; kısa açıklama sütununu gerekirse 2000 karaktere genişletir.

Personel tablosu tamamen boşsa isteğe bağlı `Veritabani/02_Ornek_Personel.sql` dosyası bir örnek personel ekler. Mevcut personel varsa kayıt eklemez. Panelde personeli listeden seç; personel numarası artık 1'e sabitlenmiş değil.

## 5 Sunumdan önce bir kez dene

Bir görev ekle, sayfayı yenile, durumu değiştir ve sil. Çakışmayı göstermek için iki sekmede aynı görevi aç; birinci sekmede güncelle, ikinci sekmede eski listeyle güncellemeyi dene. İkinci işlem 412 ile reddedilir ve güncel liste yüklenir.

## Bağlantı hataları

- 503 ve bağlantı ayarı mesajı: User Secrets adımı tamamlanmamış.
- Login failed: güncel SQL kullanıcı adı/parolasını ve sunucuyu kontrol et.
- Azure IP erişim mesajı: SQL sunucusunun Networking bölümünde bilgisayarının güncel istemci IP'sini kontrol et.
- İlk bağlantıda bekleme: serverless otomatik duraklatma açıksa veritabanının hazır olmasını bekleyip listeyi yenile.
- HTTP portu kullanımda: eski çalışan API'yi durdur.

Gerçek Azure bağlantısı ve SQL betiklerinin çalışması bu çalışma ortamından doğrulanmadı. Ayrıntılı kontrol kapsamı `DOGRULAMA_RAPORU.md` dosyasında.
