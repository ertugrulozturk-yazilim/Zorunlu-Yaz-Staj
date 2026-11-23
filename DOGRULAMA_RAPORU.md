# Sunum sürümü doğrulama raporu

Tarih: 22 Kasım 2026. Bu rapor sunum için hazırlanan son kodun doğrulamasıdır; staj günlerinde tamamlanmış yeni bir faaliyet olarak sunulmamalıdır.

## Çalıştırılan kontroller

| Kontrol | Sonuç |
| --- | --- |
| .NET 10 Release derlemesi | Başarılı; derleyici uyarıları hata kabul edilerek derlendi |
| xUnit ve ASP.NET API testleri | 23 başarılı, 0 başarısız, 0 atlanan |
| JavaScript sözdizimi | Başarılı |
| Arayüz DOM davranış senaryoları | 11 başarılı; test API verileri kullanıldı |
| Doğrudan ve transitif NuGet paket taraması | Tarama anında bildirilen bilinen güvenlik açığı yok |

## API testlerinin kapsamı

Görev oluşturma/listeleme/güncelleme/soft delete akışı; Location ve ETag; geçersiz ve olmayan personel; boş/uzun başlık; uzun açıklama; geçersiz ve eksik durum; eksik/bozuk If-Match; eski sürümle güncelleme ve silmenin reddedilmesi; tamamlanan görevin yeniden açılması; olmayan görevde 404; personel kimliğinin 1'e sabit olmaması; kullanıcı girdisinin metin olarak korunması; iç hata ayrıntılarının gizlenmesi; SQL bağlantısının TLS ayarları; eksik bağlantı ayarında 503; arayüz ve statik dosyaların aynı hosttan sunulması.

Testlerde gerçek ASP.NET MVC, controller, JSON dönüştürücü ve Business katmanı çalışır. SQL repository'leri test projesindeki bellek içi repository'lerle değiştirilir. Bu testler gerçek Azure SQL bağlantısını, SQL komutlarının sunucuda yürütülmesini veya hesabının parolasını doğrulamaz.

## Arayüz testlerinin kapsamı

İlk liste/personel yükleme ve sayaçlar; seçilen personele kayıt; mükerrer gönderimi engelleme ve form temizleme; sürümlü durum güncelleme; arama/filtre; 412 sonrası güncel veriyi yükleme; onaylı silme; HTML benzeri girdiyi textContent ile çizme; boş başlığı gönderimden önce engelleme; 503 hata mesajı; bağlantı düzeldikten sonra yenileme; zaman aşımı mesajı ve düğmeleri yeniden açma.

Bu kontroller jsdom üzerinde, taklit API yanıtlarıyla çalıştırıldı. Gerçek Chromium işlemi bu çalışma ortamında başlatılamadı. Bu nedenle gerçek tarayıcıdaki görsel yerleşim, mobil görünüm ve gerçek ağ bağlantısı doğrulanmış sayılmamalıdır.

## Yerelde tamamlanacak kontrol

1. Kendi çalışan Azure bağlantını User Secrets'a ekle.
2. Paneli http://localhost:5237 adresinde aç.
3. Kendi veritabanındaki personeli seçip deneme görevi oluştur.
4. Yenileme, durum güncelleme ve soft delete adımlarını dene.
5. İki sekmede eski sürüm çakışmasını kontrol et.

Mevcut Azure verilerine bu çalışma sırasında bağlanılmadı; SQL betikleri hesabında otomatik çalıştırılmadı. Eski ZIP'teki tablo yapısı esas alınmıştır. Veritabanını sıfırlayan bir işlem yoktur.

## Testi yeniden çalıştırma

```powershell
dotnet test StajyerTakipSistemi.sln -c Release
```

Sonuç özeti doğrulanmış test çalıştırmasından alınmıştır. Derleme klasörleri, yerel test günlükleri ve parola ZIP'e konulmamıştır.

Arayüz testlerini tekrar çalıştırmak için Node.js 22.12 veya üzeriyle `ArayuzTestleri` klasöründe `npm install` ve `npm test` kullan. Test kaynağı pakette bulunur; üretilen sonuç dosyası teslim paketine konulmaz.
