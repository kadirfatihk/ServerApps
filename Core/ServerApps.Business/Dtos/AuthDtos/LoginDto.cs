using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Dtos.AuthDtos
{
    public class LoginDto
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;
    }
}
