using Application.DTOs;
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
        public Task<User?> GetUserAsync(int id);
        public Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        public Task RevokeRefreshTokenAsync(int userId);
        public Task<ResUserStorageFileDto?> GetUserStorageStatsAsync(int userId);
        public  Task<ResUserPrivate?> GetUserPrivateProfileAsync (int userId);
    }
}
