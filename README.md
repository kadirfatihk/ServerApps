# ServerApps
## Proje Açıklaması
Bu proje, appsettings.json dosyasına yazılan sunucu bilgilerini kullanarak, belirtilen sunuculardaki IIS web sitelerini ve görev zamanlayıcısındaki (Task Scheduler) görevleri listeleyen bir araçtır. Hem API tarafı hem de MVC tabanlı UI tarafı geliştirilmiştir.

## Amaç
* appsettings.json dosyasındaki sunucu bilgileriyle uzak sunucuya bağlanır.
* Uzak sunucudaki IIS üzerindeki web sitelerini listeler.
* Uzak sunucudaki görev zamanlayıcısından (Task Scheduler) görevleri listeler.

## Teknolojiler
* .NET 8
* Entity Framework Core
* Bootstrap
* CSS ve JavaScript
* MVC
* PowerShell

## Kullanılan Kütüphaneler
* Cake.Powershell
* Microsoft.Extensions.Configuration.Binding
* Microsoft.Extensions.Hosting
* TaskScheduler
* Microsoft.EntityFrameworkCore
* Microsoft.EntityFrameworkCore.Design
* Microsoft.EntityFrameworkCore.Tools

## Kurulum
**1. Uzak Sunucu Bağlantısı** 
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

**2. Gerekli Kütüphaneleri Yükleyin**
dotnet restore

**3. Projeyi Yayınlayın**
dotnet publish -c Release

**4. Uzak Sunucuda IIS'e Ekleme**
Proje, uzak sunucudaki IIS'e bir web sitesi olarak eklendi. IIS üzerinden çalıştırıldıktan sonra başarıyla çalıştığı doğrulandı.

**5. Çalıştırma**
Projeyi çalıştırmak için aşağıdaki komutu kullanabilirsiniz:
dotnet run

**6. Kullanım**
Web sitesi üzerinden IIS web sitelerini ve görev zamanlayıcısındaki görevleri görüntüleyebilirsiniz. Kullanıcı arayüzü (UI) Bootstrap ile tasarlandı ve her iki işlem için de sonuçlar ekranda listelenir.

## Katkıda Bulunma
**1.Depoyu fork edin.**
**2. Yeni bir özellik ekleyin veya hata düzeltmesi yapın.**
**3. Değişikliklerinizi bir pull request olarak gönderin.**
