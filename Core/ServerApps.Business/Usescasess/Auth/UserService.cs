using System.Security.Cryptography;
using System.Text;
using System.Linq;
using ServerApps.Persistence.Modal;
using ServerApps.Persistence.Models;
using ServerApps.Business.Usescasess.Auth;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly DvuApplicationAuthenticationDbContext _dbContext;

    public UserService(DvuApplicationAuthenticationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public User ValidateUser(string email, string password)
    {
        var user = _dbContext.Users.FirstOrDefault(e => e.EmailAddress == email);
        if (user == null)
            return null;

        if (string.IsNullOrEmpty(user.PasswordHash))
            return null;

        if (user.PasswordHash.Length != 64) // hashlenmemiş şifre varsa
        {
            var hashed = ComputeSha256Hash(user.PasswordHash);
            user.PasswordHash = hashed;
            _dbContext.SaveChanges();
        }

        var inputHash = ComputeSha256Hash(password);

        if (user.PasswordHash == inputHash)
            return user;

        return null;
    }

    public IQueryable<User> GetAllUsers()
    {
        return _dbContext.Users;
    }

    public void AddUser(User user)
    {
        // Şifreyi hashlemeden eklemeyelim
        user.PasswordHash = ComputeSha256Hash(user.PasswordHash);

        // CreateDate alanını şu anki zaman olarak ata
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

    private string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }

    
}
