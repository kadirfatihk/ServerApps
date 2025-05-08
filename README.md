# ServerApps

## 📌 Proje Açıklaması

ServerApps, `appsettings.json` dosyasına tanımlanan uzak veya yerel sunuculara bağlanarak, IIS üzerindeki web sitelerini ve Görev Zamanlayıcı (Task Scheduler) görevlerini dinamik olarak listeleyen bir .NET 8 uygulamasıdır. Hem Web API hem de MVC tabanlı kullanıcı arayüzü ile geliştirilmiştir. 

## 🎯 Amaç

- `appsettings.json` üzerinden alınan bağlantı bilgileri ile uzak sunuculara erişmek
- Her sunucudaki IIS web sitelerini listelemek
- Web sitelerine ait Görev Zamanlayıcı (Task Scheduler) görevlerini listelemek
- Kullanıcı dostu arayüz ile verileri görsel olarak sunmak

## 🛠️ Kullanılan Teknolojiler ve Diller

### Backend
- **.NET 8**
- **C#**
- **Entity Framework Core**
- **PowerShell** (SCHTASKS.EXE ile görev listeleme)

### Frontend
- **MVC**
- **Bootstrap**
- **CSS**
- **JavaScript**

## 📚 Kullanılan Kütüphaneler

- `Cake.Powershell`
- `Microsoft.Extensions.Configuration.Binding`
- `Microsoft.Extensions.Hosting`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`
- `TaskScheduler`

## 🧱 Proje Mimarisi

Proje, **katmanlı mimari** yapısına sahiptir:

- **Core**: İş mantığını ve veri modellerini barındırır.
- **Persistence**: IIS ve Task Scheduler gibi kaynaklardan veri çekme işlemlerini gerçekleştirir.
- **WebAPI**: Sunucu bilgilerini işleyerek gerekli verileri API olarak sunar.
- **WebApp (MVC)**: Bootstrap destekli kullanıcı arayüzü ile verileri kullanıcıya sunar.

## ⚙️ Kurulum ve Kullanım

### 1. Uzak Sunucu Bilgilerini Tanımlayın

`appsettings.json` dosyasını aşağıdaki gibi yapılandırın:

```json
{
  "Applications": {
    "64 Sunucusu": [ "192.168.1.64", "Administrator", "P@ssw0rd" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

