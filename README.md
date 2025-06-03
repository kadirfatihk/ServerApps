# 🚀 ServerApps

Bu proje, `appsettings.json` dosyasına yazılan sunucu bilgilerini kullanarak, belirtilen sunuculardaki IIS web sitelerini, görev zamanlayıcısındaki (Task Scheduler) görevleri ve Event Viewer'daki olayları listeleyen ve yöneten bir araçtır. Hem API tarafı hem de MVC tabanlı UI tarafı geliştirilmiştir.

---

## 🎯 Amaç

Proje, aşağıdaki işlemleri gerçekleştirir:

- 🔗 `appsettings.json` dosyasındaki sunucu bilgileriyle uzak sunucuya bağlanır.
- 🌐 Uzak sunucudaki IIS üzerindeki web sitelerini listeler ve gelişmiş filtreleme yapabilir.
- ⏰ Uzak sunucudaki görev zamanlayıcısından (Task Scheduler) görevleri listeler, durumu değiştirir ve gelişmiş filtreleme sağlar.
- 📋 Event Viewer'daki olayları listeler ve gelişmiş filtreleme yapar.
- 🔐 Kullanıcı girişi ile kimlik doğrulama sağlar.
- 🛠️ Yönetici (admin) yetkisine sahip kullanıcıların kullanıcı yönetimi yapabilmesini sağlar.
- 🔒 Kullanıcı şifrelerini veritabanında güvenli şekilde şifreler.
- 📧 Şifremi unuttum özelliği ile kullanıcıların e-posta adreslerine doğrulama maili gönderir ve token tabanlı şifre sıfırlama ekranı sunar.

---

## 🛠️ Teknolojiler

- .NET 8
- PowerShell (uzak sunucuya bağlanmak için)
- MVC (UI tarafı)
- Bootstrap (UI tasarımı)
- CSS & JS (UI geliştirme)
- Entity Framework Core (Domain katmanı)
- Cake.Powershell (Business katmanı)
- Swashbuckle.AspNetCore (WebAPI katmanı)
- TaskScheduler (Görev Zamanlayıcı)
- Kimlik Doğrulama ve Yetkilendirme (Authentication & Authorization)
- Şifreleme ve Token Tabanlı Şifre Sıfırlama

---

## 🏗️ Mimari Yapı

Proje, katmanlı mimari kullanılarak geliştirilmiştir:

- **Core**: Business ve Domain katmanları içerir.
- **Persistence**: WebAPI ve WebMVC içerir.

### 📚 Kullanılan Kütüphaneler

- **Business Katmanı**: Cake.Powershell, Microsoft.Extensions.Configuration.Binding, Microsoft.Extensions.Hosting, TaskScheduler
- **Domain Katmanı**: Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Tools
- **WebAPI Katmanı**: TaskScheduler, Swashbuckle.AspNetCore
- **WebApp Katmanı**: TaskScheduler

---

## ✨ Özellikler ve Yenilikler

- 🔑 **Kullanıcı Girişi ve Yetkilendirme**: Kullanıcılar güvenli şekilde sisteme giriş yapabilir.
- 🔒 **Şifreleme**: Kullanıcı şifreleri veritabanında güvenli olarak şifrelenir.
- 📩 **Şifremi Unuttum**: Kullanıcılar, e-posta adreslerine gönderilen doğrulama maili üzerinden token kullanarak şifrelerini sıfırlayabilir.
- 👑 **Admin Yetkisi**: Yönetici kullanıcılar, diğer kullanıcıları yönetebilir (ekleme, silme, yetkilendirme).
- 🔍 **Gelişmiş Filtreleme**:
  - IIS Web Siteleri için filtreleme seçenekleri.
  - Görev Zamanlayıcısı (Task Scheduler) görevleri için gelişmiş filtreleme ve durum değiştirme.
  - Event Viewer olayları için gelişmiş filtreleme.
- 🔐 **Güvenli Uzak Bağlantı**: Uzak sunuculara güvenli PowerShell bağlantısı ile erişim.
- 🌐 **API ve MVC UI**: Hem RESTful API hem de kullanıcı dostu MVC tabanlı arayüz.

---

## ⚙️ Kurulum ve Kullanım

### 1. Uzak Sunucu ve Veritabanı Bağlantısı

`appsettings.json` dosyasına bağlanmak istediğiniz sunucu bilgilerini ekleyin. Örnek yapı:

```json
{
  "ConnectionStrings": {
    "DvuDb": "Server=Sunucu_Adresi; Database=Veritabanı_Adı; User Id=Kullanıcı_Adı; Password=Şifre; TrustServerCertificate=True;"
  },
  "Applications": {
    "64": [ "IP_Adresi", "Kullanıcı_Adı", "Şifre" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 2. Projeyi Çalıştırma

- 🚀 Projeyi .NET 8 ortamında derleyip çalıştırabilirsiniz.
- 👤 UI üzerinden kullanıcı girişi yaparak sunuculardaki IIS, Task Scheduler ve Event Viewer verilerine erişebilir ve filtreleyebilirsiniz.
- 🛡️ Admin yetkisi olan kullanıcılar kullanıcı yönetim paneline erişebilir.
- 🔄 Şifremi unuttum özelliği aktif olup, kullanıcılar e-posta üzerinden şifre sıfırlama işlemi yapabilir.

---

**Teşekkürler! 🙏**
