# MAUN Tomer Sertifika Doğrulama Sistemi

MAUN Tomer, Muş Alparslan Üniversitesi Türkçe Öğretimi Uygulama ve Araştırma Merkezi için geliştirilen sertifika kayıt, yönetim ve doğrulama uygulamasıdır. Uygulama, yönetici paneli üzerinden sertifika kayıtlarının yönetilmesini ve herkese açık doğrulama ekranı üzerinden sertifika bilgilerinin sorgulanmasını sağlar.

## Özellikler

- Admin kullanıcı girişi ve güvenli oturum yönetimi
- Sertifika kayıtlarını listeleme, arama, ekleme, güncelleme ve silme
- Yazdıkça filtrelenen kayıt listesi
- Excel'den toplu sertifika aktarımı
- Excel'e sertifika listesi aktarma
- Mükerrer sertifika kaydı kontrolü
- Sertifika doğrulama ekranı
- Otomatik toplam puan hesaplama
- Otomatik başarı durumu hesaplama
- SQL Server üzerinde otomatik tablo oluşturma
- Admin kullanıcılarının veritabanında hash + salt ile saklanması

## Başarı Durumu Kuralı

Sistem sertifika durumunu otomatik hesaplar:

```text
Toplam Not = Okuma + Yazma + Dinleme + Konuşma

Toplam Not > 60 ve Seviye != 0 ise BAŞARILI
Diğer tüm durumlarda BAŞARISIZ
```

## Teknolojiler

- ASP.NET Core MVC
- .NET 8
- SQL Server
- Microsoft.Data.SqlClient
- EPPlus
- Cookie Authentication
- Bootstrap

## Veritabanı

Varsayılan connection string:

```json
"Server=localhost;Database=MAUN_TOMER;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Uygulama açılışta gerekli tabloları otomatik oluşturur:

- `dbo.Tomer_CertificateInventory`
- `dbo.AdminUsers`

Varsayılan admin kullanıcısı uygulama ilk açılışında oluşturulur.

```text
Kullanıcı adı: admin
Şifre: maun2026
```

> Yayın ortamında ilk girişten sonra varsayılan şifrenin değiştirilmesi önerilir.

## Kullanım

Projeyi çalıştırmak için:

```powershell
cd C:\Users\Seyhmusa\source\repos\MAUN.Tomer.Web
dotnet run --project MAUN.Tomer.Web.csproj
```

Sertifika doğrulama ekranı:

```text
http://localhost:5274/CertificateValidation
```

Admin girişi:

```text
http://localhost:5274/Admin/Login
```

Admin sertifika yönetimi:

```text
http://localhost:5274/Admin/Certificates
```

## Excel Aktarım Formatı

Excel'den içeri aktarımda ilk satır başlık olmalıdır. Kolon sırası:

```text
Sertifika Tarihi, Kimlik/Pasaport No, Ad Soyad, Sertifika No, Seviye, Okuma, Yazma, Dinleme, Konuşma
```

Toplam puan ve başarı durumu sistem tarafından otomatik hesaplanır.

## Kurum

Muş Alparslan Üniversitesi - MAUN Tomer
