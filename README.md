# IIS Web Siteleri ve Görev Zamanlayıcı Listesi

Bu proje, `appsettings.json` dosyasına yazılan sunucu bilgilerini kullanarak, belirtilen sunuculardaki IIS web sitelerini ve görev zamanlayıcısındaki (Task Scheduler) görevleri listeleyen bir araçtır. Hem API tarafı hem de MVC tabanlı UI tarafı geliştirilmiştir.

## Amaç

Proje, aşağıdaki işlemleri gerçekleştirir:

- `appsettings.json` dosyasındaki sunucu bilgileriyle uzak sunucuya bağlanır.
- Uzak sunucudaki IIS üzerindeki web sitelerini listeler.
- Uzak sunucudaki görev zamanlayıcısından (Task Scheduler) görevleri listeler.

## Teknolojiler

- .NET 8
- PowerShell (uzak sunucuya bağlanmak için)
- MVC (UI tarafı)
- Bootstrap (UI tasarımı)
- CSS & JS (UI geliştirme)
- Entity Framework Core (Domain katmanı)
- Cake.Powershell (Business katmanı)
- Swashbuckle.AspNetCore (WebAPI katmanı)
- TaskScheduler (Görev Zamanlayıcı)

## Yapı

Proje, katmanlı mimari kullanılarak geliştirilmiştir:
- **Core**: Business ve Domain katmanları içerir.
- **Persistence**: WebAPI ve WebMVC içerir.

### Kullanılan Kütüphaneler:
- **Business Katmanı**: Cake.Powershell, Microsoft.Extensions.Configuration.Binding, Microsoft.Extensions.Hosting, TaskScheduler
- **Domain Katmanı**: Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Tools
- **WebAPI Katmanı**: TaskScheduler, Swashbuckle.AspNetCore
- **WebApp Katmanı**: TaskScheduler

## Kurulum

### 1. Uzak Sunucu Bağlantısı
`appsettings.json` dosyasına sunucu bilgilerini ekleyin. Örneğin:

```json
{
  "Applications": {
    "64 Sunucusu": [ "uzak sunucu ıp", "username", "password" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
