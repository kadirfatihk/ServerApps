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
    "Sunucu Adı": [ "IP Adresi", "Kullanıcı Adı", "Şifre" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

### 2. Gerekli Paketleri Yükleyin
dotnet restore

### 3. Projeyi Yayınlayın
dotnet publish -c Release

### 4. IIS Üzerinden Web Sitesi Olarak Yayınlayın
- ** Yayınladığınız dosyaları uzak sunucuya kopyalayın
- ** IIS üzerinden yeni bir web sitesi oluşturun ve bu dizini seçin
- ** Web sitesini başlatın ve kontrol edin

### Web Arayüzü Kullanımı
- ** Web sitesine tarayıcı üzerinden erişin
- ** Ana sayfada sunucular listelenir
- ** İlgili sunucuya tıkladığınızda, o sunucudaki IIS web siteleri ve görev zamanlayıcı görevleri listelenir
- ** Tüm veriler canlı olarak sunuculardan çekilir, veritabanı kullanılmaz

## 🤝 Katkıda Bulunma
- ** Bu projeyi fork edin
- ** Yeni bir branch oluşturun (feature/yeniozellik)
- ** Değişikliklerinizi yapın ve commit edin
- ** Pull Request gönderin

