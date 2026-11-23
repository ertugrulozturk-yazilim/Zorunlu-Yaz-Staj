# Sekiz dakikalık staj sunumu

Bu metin sunum için düzenlenen son kodla uyumludur. Stajda yapılan çalışmalar ile sunum öncesi tamamlanan düzenlemeleri zaman bakımından ayır. Başlıkları ve ekran notlarını sesli okuma. API ve siteyi önceden aç; güncel bağlantınla bir deneme yap. Bağlantı parolasını ekranda gösterme.

## 1 Kendini ve stajını tanıt

Süre 00:00–00:40

Merhaba hocalarım. Ben Ertuğrul Öztürk. Yirmi iş günlük mesleki stajımı Albil’de, Mobil Yazılım departmanında tamamladım. Stajın ilk bölümünde şirketin işleyişini ve yazılım geliştirme süreçlerini tanıdım. Git, SQL, API kullanımı ve arayüz geliştirme üzerine küçük çalışmalar yaptım. İkinci bölümde bu bilgileri bir araya getirerek Stajyer Görev Takip Sistemi geliştirdim. Sunumda önce hazırlık çalışmalarımı, ardından uygulamanın özellikle backend yapısını ve kısa bir demosunu anlatacağım.

## 2 İlk günlerde öğrendiklerini anlat

Süre 00:40–01:30

İlk günlerde çevik geliştirme yaklaşımını, kullanıcı hikâyelerini ve kabul kriterlerini araştırdım. Örnek olarak çevrimdışı çalışan bir dil öğrenme uygulaması için kullanıcının ihtiyacını, teknik bağımlılıkları ve işin ne zaman tamamlanmış sayılacağını tanımladım. Böylece kodlamaya başlamadan önce gereksinimi netleştirmenin önemini gördüm.
Git çalışmalarında depo klonlama, ayrı dal açma, değişiklikleri commit ile kaydetme ve uzak depoya gönderme işlemlerini uyguladım. Burada dal açmanın ana kodu etkilemeden çalışmayı, commit geçmişinin de değişiklikleri takip etmeyi sağladığını öğrendim. Ayrıca Postman üzerinden örnek API istekleri gönderdim; interface kullanımını pekiştirmek için küçük bir vergi hesaplama servisi üzerinde çalıştım.

## 3 SQL ve veritabanı tasarımını açıkla

Süre 01:30–02:20

Veritabanı çalışmalarına SQL Server üzerinde veri ekleme, okuma, güncelleme ve silme sorguları yazarak başladım. Önce personel, departman, pozisyon, izin ve mesai süreçlerini içeren bir tasarım alıştırması yaptım. Ardından görev takip projesinin veritabanını oluşturdum.
Görev takip projesinin paylaştığım şemasında departmanlar, pozisyonlar, personeller ve görevler tabloları bulunuyor. Bir personelin birden fazla görevi olabiliyor. Görev tablosundaki PersonelId, personel tablosuna yabancı anahtarla bağlı. Böylece olmayan bir personele görev atanması engelleniyor. Ayrıca durum alanını Yapılacak, Devam Ediyor ve Tamamlandı değerleriyle sınırlandıran bir CHECK kısıtı kullanıyorum.

## 4 Backend yapısını anlat

Süre 02:20–03:45

Ekran notu: Visual Studio’da önce controller, sonra servis ve repository dosyalarını göster. Kodun her satırını okumadan görevlerini açıkla.

Backend tarafını C# ve ASP.NET Core Web API ile geliştirdim. Yapıyı API, Business, DataAccess ve Entities projelerine ayırdım. Controller HTTP isteklerini karşılıyor. Business katmanındaki servisler iş kurallarını uyguluyor. SQL sorguları ise DataAccess içindeki repository sınıflarında bulunuyor.
Görev servisi doğrudan SQL komutu çalıştırmıyor; IGorevRepository arayüzünü kullanıyor. Somut repository ve servisleri Program.cs dosyasında dependency injection ile kaydediyorum. Böylece controller, iş kuralları ve veritabanı erişiminin sorumlulukları ayrılmış oluyor.
Entities katmanında görev modeli, ayrı ekleme ve güncelleme DTO’ları ve GorevDurumu enum’u bulunuyor. Enum ile durumları belirli seçeneklerle temsil ediyorum. DTO’larla da istemciden hangi alanların alınacağını belirliyorum.
Veri erişiminde ADO.NET ve parametreli sorgular kullanıyorum. İşlemler async ve await ile yürütülüyor; istek iptal edilirse iptal bilgisi veri erişimine kadar aktarılıyor. Bu sürümde ORM kullanmıyorum.

## 5 API ve istek akışını açıkla

Süre 03:45–04:35

API’de GET ile görevleri listeliyorum, POST ile ekliyorum, PUT ile durumlarını değiştiriyorum ve DELETE ile soft delete yapıyorum. Başarılı eklemede 201, silmede 204 dönüyorum.
Arayüzde HTML, CSS ve JavaScript kullandım. fetch ile bilgileri JSON olarak gönderiyorum. İstek controller’dan servise, servisten repository’ye ve Azure SQL’e ulaşıyor. Yanıt dönünce arayüz güncelleniyor.
Arayüz ve API aynı adresten sunulduğu için ayrı bir Live Server çalıştırmıyorum. Personel bilgisini de sabit bir numarayla göndermek yerine veritabanından gelen listeden seçiyorum.

## 6 Azure ve sorgu güvenliğini anlat

Süre 04:35–05:15

Veritabanını Azure SQL Database üzerinde tutuyorum; API bu gösterimde kendi bilgisayarımda çalışıyor. Bağlantı bilgisini User Secrets üzerinden okuyorum.
SQL bağlantısında TLS ve sertifika doğrulaması kullanıyorum. Bu, hazır güvenli bağlantı mekanizması; kendime ait bir şifreleme algoritması yazmadım.
Ayrıca eşzamanlı değişiklikleri RowVersion ile kontrol ediyorum. İstemci okuduğu sürümü If-Match başlığında gönderiyor. SQL güncellemesi yalnızca aynı sürüm hâlâ geçerliyse gerçekleşiyor. Başka biri kaydı değiştirmişse işlem 412 ile reddediliyor.

## 7 Kısa demo yap

Süre 05:15–06:15

Ekran notu: Personel seç, örnek görev ekle, sayfayı yenile, durumu değiştir ve sil. Bu gösterimi bir dakikada bitir. İki sekmeli sürüm çakışmasını ancak soru gelirse veya zaman kalırsa göster.

Şimdi uygulamanın temel işlemlerini göstereceğim. Örnek bir görev ekliyorum. Görev listeye geliyor; sayfayı yenilediğimde de görünmesi, kaydın veritabanında tutulduğunu gösteriyor.
Görevin durumunu önce Devam Ediyor, ardından Tamamlandı olarak değiştiriyorum. Son olarak görevi siliyorum. Burada kayıt veritabanından fiziksel olarak kaldırılmıyor; IsDeleted alanı bir yapılıyor. Listeleme sorgusu yalnızca IsDeleted değeri sıfır olan kayıtları getirdiği için görev ekrandan kayboluyor. Bu yönteme soft delete deniyor.

## 8 Testleri ve sorun çözme yaklaşımını anlat

Süre 06:15–07:05

Stajda işlemleri Postman ve arayüz üzerinden manuel olarak test ettim. Sunuma hazırladığım son sürümde otomatik API testleri de bulunuyor. Bunlar geçersiz veri, eksik sürüm ve eski kayıtla güncelleme gibi senaryoları kontrol ediyor.
Örneğin görevler gelmediğinde tarayıcıda pending görmüştüm. Visual Studio’daki mesaj veritabanında oturum açmanın reddedildiğini gösteriyordu. Böylece hatayı arayüzden ayırıp bağlantıya odaklanabildim. Bu deneyim, istemciyi, API’yi ve veritabanını ayrı incelemenin önemini gösterdi.

## 9 Dokümantasyon ve kazanımlarla bitir

Süre 07:05–08:00

Son aşamada API adreslerini, istek gövdelerini, durum kodlarını ve veritabanı yapısını dokümante ettim. Kod inceleme ve alternatif tasarımlar için yapay zekâ desteğinden yararlandım. Son sürümde repository, enum ve sürüm kontrolünün uygulamadaki karşılığını kod üzerinden gösterebiliyorum.
Ortaya çıkan ürün; görev ekleme, listeleme, durum değiştirme ve soft delete işlemlerini karşılayan bir MVP. Kullanıcı girişi ve rol yetkilendirmesi gibi geliştirme alanları bulunuyor.
Stajın bana en önemli katkısı, bir gereksinimi çalışan uygulamaya dönüştürürken veritabanı, API, hata ayıklama, test ve dokümantasyonu birlikte ele almayı öğrenmek oldu. Teşekkür ederim.

# Kod üzerinde sorulabilecek kısa sorular

| Soru | Kısa cevap |
| --- | --- |
| Repository ne sağlıyor? | Veri erişimini servis iş kurallarından ayırıyor. SQL sorguları SqlGorevRepository içinde. |
| Neden interface kullanıyorsun? | Servis somut SQL sınıfına bağımlı kalmıyor; testte başka repository verilebiliyor. |
| Scoped ne demek? | Servis ve repository bir HTTP isteğinin kapsamında oluşturulup kullanılıyor. |
| DTO neden ayrı? | İstek ve yanıt için gerekli alanları belirliyorum; istemci görev kimliği veya RowVersion oluşturmuyor. |
| Enum neden var? | Kod içinde tanımlı üç durumla çalışıyorum. JSON dönüştürücüsü ve SQL eşlemesi Türkçe değerleri koruyor. |
| RowVersion tarih mi? | Hayır, SQL Server’ın değişikliklerde yenilediği 8 baytlık sürüm değeri. |
| Çakışma nasıl önleniyor? | UPDATE ve soft delete WHERE şartında Id, IsDeleted ve RowVersion birlikte kontrol ediliyor. Eski sürüm yazamıyor. |
| SQL injection önlemin ne? | Kullanıcı değerleri türü ve uzunluğu belirtilen parametrelerle gönderiliyor. |
| Şifrelemeyi sen mi yazdın? | SQL bağlantısında TLS ve sertifika doğrulaması kullanıyorum; özel kriptografi uygulamadım. |
| ORM var mı? | Bu projede ADO.NET var. Entity Framework veya başka bir ORM kullanılmıyor. |
| Testler canlı Azure’u doğruluyor mu? | Hayır. API testlerinde SQL repository yerine test repository kullanılıyor. Canlı bağlantıyı kendi bilgisayarımda ayrıca deniyorum. |
| Authentication var mı? | Hayır. Kullanıcı girişi ve rol yetkilendirmesi gelecekteki geliştirme alanları. |

# Staj defterindeki teknik ifadeler

Repository, enum ve RowVersion kontrolü bu son sürümde uygulanmıştır. Şifreleme için doğru ifade “SQL bağlantısında TLS ve sunucu sertifikası doğrulaması kullanıldı” olmalıdır. ORM için bu projedeki doğru teknoloji ADO.NET’tir; ORM araştırmasıyla projede ORM kullanılmasını birbirine karıştırma. İlk sürümdeki CORS çalışmasını açıklayabilirsin; son sürümde arayüz ve API aynı origin üzerinden çalışır.

Sunum öncesi yapılan yeni düzenlemeleri, stajın belirli bir gününde yapılmış gibi anlatma. Final kodunun doğrulama kapsamı DOGRULAMA_RAPORU.md içinde yazılıdır.

# Prova

Bir kez kronometreyle oku. 05:15 civarında demoya, 07:05 civarında kapanışa geç. Bir yerde takılırsan ayrıntıyı kısaltıp ana akışa dön: istek, controller, servis, repository, veritabanı. Toplam süreyi 8 dakikanın altında tutarak komisyon sorularına yer bırak.

