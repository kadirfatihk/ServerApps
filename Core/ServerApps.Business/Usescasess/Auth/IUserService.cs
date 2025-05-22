using Microsoft.EntityFrameworkCore;
using ServerApps.Business.Dtos.AuthDtos;
using ServerApps.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Usescasess.Auth
{
    public interface IUserService
    {
        User ValidateUser(string email, string password);
        IQueryable<User> GetAllUsers();
        void AddUser(User user);
        void DeleteUser(int userId);
        void UpdateUserAdminStatus(int userId, bool isAdmin);
    }
}
