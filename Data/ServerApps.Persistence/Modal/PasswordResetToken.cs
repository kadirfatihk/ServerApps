using ServerApps.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Persistence.Modal
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        // Tokenu ilişkilendireceğimiz kullanıcı
        public int UserId { get; set; }

        public string Token { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        // Navigation property - kullanıcı ile bağlantı
        public virtual User User { get; set; } = null!;
    }
}
