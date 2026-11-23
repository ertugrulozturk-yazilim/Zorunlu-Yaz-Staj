# Görev takip API dokümantasyonu

Temel adres: `http://localhost:5237`. İstek ve yanıt alanları camelCase kullanır. Durumlar JSON'da yalnızca `Yapılacak`, `Devam Ediyor` ve `Tamamlandı` değerleriyle temsil edilir.

| Metot | Yol | Davranış |
| --- | --- | --- |
| GET | /health | API sürecinin çalıştığını bildirir; veritabanı bağlantısını test etmez |
| GET | /api/Personel | Personelleri departman ve pozisyonlarıyla listeler |
| GET | /api/Gorev | Silinmemiş görevleri yeni kayıttan eskiye listeler |
| GET | /api/Gorev/{id} | Bir görevi döndürür; ETag başlığında sürümünü bildirir |
| POST | /api/Gorev | Yeni görev oluşturur; 201, Location, ETag ve oluşturulan görevi döndürür |
| PUT | /api/Gorev/{id} | Yalnızca görev durumunu günceller; If-Match zorunludur |
| DELETE | /api/Gorev/{id} | Soft delete yapar; If-Match zorunludur |

## Yeni görev oluşturma

Önce GET /api/Personel ile geçerli personel kimliğini öğren. Aşağıdaki 7 bir örnektir; kendi veritabanındaki kimliği kullan.

```http
POST /api/Gorev
Content-Type: application/json
```

```json
{
  "personelId": 7,
  "baslik": "API testlerini tamamla",
  "aciklama": "Ekleme ve güncelleme yanıtlarını kontrol et."
}
```

Başlık 1–150 karakter, açıklama en fazla 2000 karakterdir. Boşluklardan oluşan başlık geçersizdir. Açıklama isteğe bağlıdır. Yeni görev Yapılacak durumunda başlar. Kimlik, tarih ve sürüm sunucu tarafından oluşturulur.

## Görev yanıtı

```json
{
  "id": 12,
  "personelId": 7,
  "baslik": "API testlerini tamamla",
  "aciklama": "Ekleme ve güncelleme yanıtlarını kontrol et.",
  "durum": "Yapılacak",
  "olusturmaTarihi": "2026-11-22T10:00:00",
  "bitisTarihi": null,
  "rowVersion": "AAAAAAAAAAE="
}
```

Sayılar ve sürüm değeri örnektir. SQL datetime2 değerleri saat dilimi ofseti içermez; yeni kayıtlar repository tarafından SYSUTCDATETIME ile oluşturulur. Eski kayıtların saat değerleri otomatik olarak dönüştürülmez.

## Durum güncelleme

Önceki yanıtın rowVersion değerini çift tırnak içinde If-Match başlığına koy:

```http
PUT /api/Gorev/12
Content-Type: application/json
If-Match: "AAAAAAAAAAE="
```

```json
{ "durum": "Devam Ediyor" }
```

Başarıda 200, güncellenen görev ve yeni ETag döner. Bir sonraki değişiklikte YENİ rowVersion kullanılmalıdır. Tamamlandı durumunda bitiş tarihi atanır; başka duruma dönülünce bitiş tarihi temizlenir.

Bu sürümde eski API'nin doğrudan JSON metni olan `"Tamamlandı"` gövdesi kullanılmaz. Yeni wwwroot arayüzü güncel sözleşmeye uygundur.

## Silme

```http
DELETE /api/Gorev/12
If-Match: "GUNCEL_BASE64_SURUM"
```

Gövde gönderilmez. Başarıda 204 döner. SQL'de yalnızca IsDeleted=1 yapılır; kayıt fiziksel olarak korunur. Silinen görev normal GET sorgularında görünmez. Geri alma uç noktası yoktur.

## Sürüm kontrolünün sonucu

| Durum | HTTP kodu |
| --- | --- |
| Geçersiz gövde, durum, kimlik veya sürüm biçimi | 400 |
| Görev bulunamadı veya silinmiş | 404 |
| Başka işlem kaydı değiştirmiş; If-Match artık güncel değil | 412 |
| If-Match başlığı gönderilmemiş | 428 |
| Veritabanı yapılandırması veya erişimi başarısız | 503 |
| Beklenmeyen sunucu hatası | 500 |

Yalnızca tek, çift tırnaklı, 8 baytlık Base64 sürüm kabul edilir. Weak ETag, yıldız ve çoklu ETag bu API'de desteklenmez. Hatalar ProblemDetails yapısındadır. Model doğrulama hatalarında errors sözlüğü, diğer hatalarda detail alanı bulunur. İç istisna ayrıntıları ve parola istemciye gönderilmez.

## İki sekmede çakışma gösterimi

A ve B sekmelerinde aynı listeyi aç. A sekmesinden görevi güncelle. B sekmesini yenilemeden aynı görevi güncellemeyi dene. B'nin eski sürümü SQL WHERE şartını geçemez. API 412 döndürür; B listesi yenilenir. Başkasının değişikliği sessizce ezilmez.

Personel CRUD, görev başlığı düzenleme, kullanıcı oturumu ve rol yönetimi bu API'nin kapsamı dışındadır.
