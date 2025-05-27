using Microsoft.Extensions.Caching.Memory;
using ServerApps.Business.Dtos.AuthDtos;
using ServerApps.Business.Usescasess.Auth;
using ServerApps.Persistence.Modal;
using ServerApps.Persistence.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http;

public class UserService : IUserService
{
    private readonly DvuApplicationAuthenticationDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;

    // Token ve expiry bilgisini tutan dictionary (ekstra expiry eklenebilir)
    private readonly Dictionary<string, (string Token, DateTime Expiry)> _resetTokens = new();

    public UserService(DvuApplicationAuthenticationDbContext dbContext, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public User ValidateUser(string email, string password)
    {
        var user = _dbContext.Users.FirstOrDefault(e => e.EmailAddress == email);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        // Eğer eski şifre formatı ise, hashleyip güncelle
        if (user.PasswordHash.Length != 64)
        {
            user.PasswordHash = ComputeSha256Hash(user.PasswordHash);
            _dbContext.SaveChanges();
        }

        var inputHash = ComputeSha256Hash(password);
        return user.PasswordHash == inputHash ? user : null;
    }

    public IQueryable<User> GetAllUsers() => _dbContext.Users;

    public void AddUser(User user)
    {
        user.PasswordHash = ComputeSha256Hash(user.PasswordHash);
        user.CreateDate = DateTime.Now;
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
    }

    public void DeleteUser(int userId)
    {
        var user = _dbContext.Users.Find(userId);
        if (user != null)
        {
            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
        }
    }

    public void UpdateUserAdminStatus(int userId, bool isAdmin)
    {
        var user = _dbContext.Users.Find(userId);
        if (user != null)
        {
            user.IsAdmin = isAdmin;
            _dbContext.SaveChanges();
        }
    }

    public bool SendResetPasswordEmail(string email, out string errorMessage)
    {
        errorMessage = null;

        var user = _dbContext.Users.FirstOrDefault(u => u.EmailAddress == email);
        if (user == null)
        {
            errorMessage = "E-posta adresi sistemde bulunamadı.";
            return false;
        }

        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddHours(1);

        // Token ve expiry'yi cache'de sakla, süresi dolunca otomatik silinir
        _memoryCache.Set(email, (token, expiry), expiry);

        var resetLink = $"http://localhost:5243/Auth/ResetPassword?email={email}&token={token}";

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
            using var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://192.168.161.120:4104/api/Mails/SendSmtpOutlookAsync"),
                Content = new StringContent(JsonSerializer.Serialize(mailDto), Encoding.UTF8, "application/json")
            };

            var response = client.Send(request);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                errorMessage = $"Mail gönderim hatası: {responseContent}";
                return false;
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Mail gönderim hatası: {ex.Message}";
            return false;
        }

        return true;
    }

    public bool ResetPassword(string email, string token, string newPassword, out string errorMessage)
    {
        errorMessage = null;

        if (!_memoryCache.TryGetValue<(string Token, DateTime Expiry)>(email, out var tokenInfo))
        {
            errorMessage = "Invalid or expired token.";
            return false;
        }

        if (tokenInfo.Token != token || tokenInfo.Expiry < DateTime.UtcNow)
        {
            errorMessage = "Invalid or expired token.";
            return false;
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.EmailAddress == email);
        if (user == null)
        {
            errorMessage = "User not found.";
            return false;
        }

        var newPasswordHash = ComputeSha256Hash(newPassword);

        // Yeni şifre eski şifre ile aynı mı kontrol et
        if (user.PasswordHash == newPasswordHash)
        {
            errorMessage = "Your password cannot be the same as your old password.";
            return false;
        }

        // Şifreyi güncelle
        user.PasswordHash = newPasswordHash;
        _dbContext.SaveChanges();

        // Cache’den token sil
        _memoryCache.Remove(email);

        return true;
    }


    private string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var b in bytes)
            builder.Append(b.ToString("x2"));
        return builder.ToString();
    }
}
