using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace ServerApps.Business.Usescasess
{
    public static class PowerShellHelper
    {
        // Uzak sunucudaki Powershell komutlarını çalıştıran metot
        public static List<PSObject> InvokeRemoteCommand(string ip, string username, string password, string script)
        {
            try
            {
                Console.WriteLine($"[INFO] Uzak sunucuya bağlanılıyor: {ip}"); // Bağlantı bilgisi loglanır

                var connectionInfo = new WSManConnectionInfo(
                    new Uri($"https://{ip}:5985/wsman"), // Uzak sunucunun IP adresi ve wsman portu
                    "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", // Powershell bağlantısı için kullanılan URI
                    new PSCredential(username, CreateSecureString(password)) // Kullanıcı adı ve şifre bilgileri
                )
                {
                    AuthenticationMechanism = AuthenticationMechanism.Default, // Varsayılan kimlik doğrulama mekanizması
                    OperationTimeout = 4 * 60 * 1000, // Komut için operasyon süresi
                    OpenTimeout = 1 * 60 * 1000 // Bağlantı için açılış süresi
                };

                using var runspace = RunspaceFactory.CreateRunspace(connectionInfo); // Runspace oluşturuluyor
                runspace.Open(); // Bağlantıyı açar

                using var pipeline = runspace.CreatePipeline(); // Powershell pipeline'ı oluşturuluyor
                pipeline.Commands.AddScript(script); // Verilen Powershell komutu eklenir
                var results = pipeline.Invoke(); // Komut çalıştırılır

                runspace.Close(); // Runspace kapatılır

                Console.WriteLine($"[SUCCESS] {ip} sunucusundan {results.Count} sonuç döndü."); // Başarı durumu loglanır
                return results.ToList(); // Sonuçlar döndürülür
            }
            catch (Exception ex)
            {
                var msg = $"[ERROR] {ip} adresine bağlanırken hata oluştu: {ex.Message}"; // Hata mesajı
                Console.WriteLine(msg); // Hata loglanır
                throw new Exception(msg, ex); // Hata fırlatılır
            }
        }

        // Şifreyi SecureString formatına dönüştüren yardımcı metot
        private static SecureString CreateSecureString(string password)
        {
            var securePassword = new SecureString(); // SecureString nesnesi oluşturuluyor
            foreach (var c in password)
                securePassword.AppendChar(c); // Şifrenin her karakteri eklenir
            securePassword.MakeReadOnly(); // SecureString sadece okunabilir hale getirilir
            return securePassword; // SecureString döndürülür
        }
    }

}
