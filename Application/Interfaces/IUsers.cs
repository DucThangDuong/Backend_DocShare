using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUsers
    {
        public Task<bool> ExistEmailAsync(string email);
        public Task<bool> CreateUserAsync(User user);
        public Task<User?> GetByEmailAsync(string email);
        public Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate);
    }
}
