using Microsoft.Extensions.Caching.Memory; // Bellek içi önbellek kullanımı için gerekli
using ServerApps.Business.Dtos.AuthDtos;
using ServerApps.Business.Usescasess.Auth;
using ServerApps.Persistence.Modal;
using ServerApps.Persistence.Models;
using System.Security.Cryptography; // SHA256 için
using System.Text;
using System.Text.Json;
using System.Net.Http;

public class UserService : IUserService
{
    private readonly DvuApplicationAuthenticationDbContext _dbContext; // Veritabanı işlemleri için context
    private readonly IMemoryCache _memoryCache; // Şifre sıfırlama token'ları için önbellek

    // Token ve geçerlilik süresini saklamak için dictionary (ekstra kontrol amaçlı, aslında cache kullanılıyor)
    private readonly Dictionary<string, (string Token, DateTime Expiry)> _resetTokens = new();

    public UserService(DvuApplicationAuthenticationDbContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public User ValidateUser(string email, string password)
    {
        var user = _dbContext.Users.FirstOrDefault(e => e.EmailAddress == email); // E-posta ile kullanıcıyı bul
        if (user == null || string.IsNullOrEmpty(user.PasswordHash)) // Kullanıcı yoksa veya şifresi boşsa
            return null;

        // Eski şifre formatını SHA256'a çevir
        if (user.PasswordHash.Length != 64)
        {
            user.PasswordHash = ComputeSha256Hash(user.PasswordHash);
            _dbContext.SaveChanges(); // Veritabanını güncelle
        }

        var inputHash = ComputeSha256Hash(password); // Girilen şifreyi hashle
        return user.PasswordHash == inputHash ? user : null; // Hash'ler eşleşiyorsa kullanıcıyı döndür
    }

    public IQueryable<User> GetAllUsers() => _dbContext.Users; // Tüm kullanıcıları getir

    public void AddUser(User user)
    {
        user.PasswordHash = ComputeSha256Hash(user.PasswordHash); // Şifreyi hashle
        user.CreateDate = DateTime.Now; // Oluşturulma tarihini ayarla
        _dbContext.Users.Add(user); // Veritabanına ekle
        _dbContext.SaveChanges(); // Değişiklikleri kaydet
    }

    public void DeleteUser(int userId)
    {
        var user = _dbContext.Users.Find(userId); // Kullanıcıyı ID ile bul
        if (user != null)
        {
            _dbContext.Users.Remove(user); // Kullanıcıyı sil
            _dbContext.SaveChanges(); // Değişiklikleri kaydet
        }
    }

    public void UpdateUserAdminStatus(int userId, bool isAdmin)
    {
        var user = _dbContext.Users.Find(userId); // Kullanıcıyı ID ile bul
        if (user != null)
        {
            user.IsAdmin = isAdmin; // Admin yetkisini güncelle
            _dbContext.SaveChanges(); // Değişiklikleri kaydet
        }
    }

    public bool SendResetPasswordEmail(string email, out string errorMessage)
    {
        errorMessage = null;

        var user = _dbContext.Users.FirstOrDefault(u => u.EmailAddress == email); // Kullanıcıyı e-posta ile bul
        if (user == null)
        {
            errorMessage = "E-posta adresi sistemde bulunamadı.";
            return false;
        }

        var token = Guid.NewGuid().ToString(); // Benzersiz token oluştur
        var expiry = DateTime.UtcNow.AddHours(1); // 1 saatlik geçerlilik süresi

        // Token’ı önbelleğe kaydet
        _memoryCache.Set(email, (token, expiry), expiry);

        // Şifre sıfırlama bağlantısı oluştur
        var resetLink = $"http://localhost:5243/Auth/ResetPassword?email={email}&token={token}";

        // Gönderilecek e-posta içeriği
        var mailDto = new
        {
            Subject = "Şifre Sıfırlama",
            Body = $@"
            <p>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
            <a href='{resetLink}' style='display:inline-block;padding:10px 20px;background-color:#007bff;color:#fff;text-decoration:none;border-radius:5px;'>Şifreyi Sıfırla</a>",
            ToEmailAddress = new List<string> { email }
        };

        try
        {
            using var client = new HttpClient(); // HTTP istemcisi oluştur
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post, // POST isteği
                RequestUri = new Uri("http://192.168.161.120:4104/api/Mails/SendSmtpOutlookAsync"), // Mail API endpoint'i
                Content = new StringContent(JsonSerializer.Serialize(mailDto), Encoding.UTF8, "application/json") // JSON içeriği
            };

            var response = client.Send(request); // HTTP isteğini gönder

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                errorMessage = $"Mail gönderim hatası: {responseContent}";
                return false;
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Mail gönderim hatası: {ex.Message}"; // İstisna durumunda hata mesajı
            return false;
        }

        return true;
    }

    public bool ResetPassword(string email, string token, string newPassword, out string errorMessage)
    {
        errorMessage = null;

        // Cache'den token bilgisi al
        if (!_memoryCache.TryGetValue<(string Token, DateTime Expiry)>(email, out var tokenInfo))
        {
            errorMessage = "Invalid or expired token.";
            return false;
        }

        // Token geçerlilik süresi veya eşleşme kontrolü
        if (tokenInfo.Token != token || tokenInfo.Expiry < DateTime.UtcNow)
        {
            errorMessage = "Invalid or expired token.";
            return false;
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.EmailAddress == email); // Kullanıcıyı bul
        if (user == null)
        {
            errorMessage = "User not found.";
            return false;
        }

        var newPasswordHash = ComputeSha256Hash(newPassword); // Yeni şifreyi hashle

        // Aynı şifre mi kontrol et
        if (user.PasswordHash == newPasswordHash)
        {
            errorMessage = "Your password cannot be the same as your old password.";
            return false;
        }

        // Şifreyi güncelle
        user.PasswordHash = newPasswordHash;
        _dbContext.SaveChanges(); // Değişiklikleri kaydet

        // Cache'den token'ı sil
        _memoryCache.Remove(email);

        return true;
    }

    private string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create(); // SHA256 nesnesi oluştur
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData)); // Veriyi byte dizisine çevirip hashle
        var builder = new StringBuilder();
        foreach (var b in bytes)
            builder.Append(b.ToString("x2")); // Hex formatında stringe çevir
        return builder.ToString(); // Hash'lenmiş string'i döndür
    }

    public bool HasAnyUser()
    {
        return _dbContext.Users.Any();
    }

    public void CreateUser(string email, string plainPassword, string firstName = null, string lastName = null, string JobTitle = null, bool isAdmin = false)
    {
        var user = new User
        {
            EmailAddress = email,
            PasswordHash = plainPassword, // AddUser içinde hash'lenecek
            FirstName = firstName,
            LastName = lastName,
            JobTitle = JobTitle,
            IsAdmin = isAdmin,
            CreateDate = DateTime.Now
        };

        AddUser(user);
    }
}
