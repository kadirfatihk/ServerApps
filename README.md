# ServerApps

---

## 📌 Proje Açıklaması
ServerApps, `appsettings.json` dosyasına tanımlanan uzak veya yerel sunuculara bağlanarak, IIS üzerindeki web sitelerini ve Görev Zamanlayıcı (Task Scheduler) görevlerini dinamik olarak listeleyen bir .NET 8 uygulamasıdır. Hem Web API hem de MVC tabanlı kullanıcı arayüzü ile geliştirilmiştir.

---

## 🎯 Amaç
- 🔗 **Uzak Sunucu Bağlantıları**: `appsettings.json` üzerinden alınan bağlantı bilgileri ile uzak sunuculara erişim sağlanır.
- 🌐 **IIS Web Siteleri Listeleme**: Her sunucudaki IIS web siteleri dinamik olarak listelenir.
- 🕒 **Görev Zamanlayıcısı (Task Scheduler) Görevleri**: Web sitelerine ait görev zamanlayıcısındaki görevler listelenir.
- 🎨 **Kullanıcı Dostu Arayüz**: Veriler görsel olarak kullanıcı dostu bir şekilde sunulur.

---

## 🔍 Arama Özelliği
- Kullanıcılar, IP adresi, port numarası, uygulama adı, görev durumu (Ready, Running, vb.) gibi kriterlere göre arama yapabilirler.
- Arama, kullanıcıya hızlı bir şekilde ilgili sunucu ve görevleri bulma imkanı sunar.
- Esnek Arama: Arama sırasında, yalnızca IP, port, uygulama adı gibi doğrudan bilgilere değil, görev durumlarına veya görev adı içeriğine göre de sonuçlar dönebilir. Örneğin:
   Bu özellik hem IIS üzerindeki web siteleri hem de Görev Zamanlayıcısı görevleri için geçerlidir.

---
## 🛠️ Kullanılan Teknolojiler

### Backend Teknolojileri 🔧
- **.NET 8**
- **Entity Framework Core**
- **PowerShell**

### Frontend Teknolojileri 🎨
- **MVC**
- **Bootstrap**

---

## 💻 Kullanılan Diller

- **C#**
- **CSS**
- **JavaScript**

---

## 📚 Kullanılan Kütüphaneler
- `Cake.Powershell`
- `Microsoft.Extensions.Configuration.Binding`
- `Microsoft.Extensions.Hosting`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`
- `TaskScheduler`

---

## 🧱 Proje Mimarisi
Proje, **katmanlı mimari** yapısına sahiptir:

- **Core**: İş mantığını ve veri modellerini barındırır.
- **Persistence**: IIS ve Task Scheduler gibi kaynaklardan veri çekme işlemlerini gerçekleştirir.
- **WebAPI**: Sunucu bilgilerini işleyerek gerekli verileri API olarak sunar.
- **WebApp (MVC)**: Bootstrap destekli kullanıcı arayüzü ile verileri kullanıcıya sunar.

---

## ⚙️ Kurulum ve Kullanım

### 1. Uzak Sunucu Bilgilerini Tanımlayın
`appsettings.json` dosyasını aşağıdaki gibi yapılandırın:
```
{
  "Applications": {
    "SUNUCU_ADI": [ "IP", "KULLANICI_ADI", "ŞİFRE" ]
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
### 2. Gerekli Paketleri Yükleyin
Proje bağımlılıklarını yüklemek için terminal üzerinden aşağıdaki komutu çalıştırın:
dotnet restore

### 3. Projeyi Yayınlayın
Projeyi yayınlamak için aşağıdaki komutu kullanın:
dotnet publish -c Release

### 4. IIS Üzerinden Web Sitesi Olarak Yayınlayın
- Yayınladığınız dosyaları uzak sunucuya kopyalayın.
- IIS üzerinden yeni bir web sitesi oluşturun ve bu dizini seçin.
- Web sitesini başlatın ve kontrol edin.

### 5. Web Arayüzü Kullanımı
- Web sitesine tarayıcı üzerinden erişin.
- Ana sayfada sunucular listelenir.
- İlgili sunucuya tıkladığınızda, o sunucudaki IIS web siteleri ve görev zamanlayıcı görevleri listelenir.
- Tüm veriler canlı olarak sunuculardan çekilir, veritabanı kullanılmaz.

---

## 🤝 Katkıda Bulunma
- Bu projeyi fork edin.
- Yeni bir branch oluşturun (feature/yeniozellik).
- Değişikliklerinizi yapın ve commit edin.
- Pull Request gönderin.

---

## Proje Görselleri
![Anasayfa](https://www.example.com/gorsel.png)
![IIS Sayfası](https://www.example.com/gorsel.png)
![Task Sayfası](https://www.example.com/gorsel.png)

