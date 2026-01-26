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
        public Task<ResUserPrivate?> GetUserPrivateProfileAsync(int userId);
        public Task<bool> UpdateUserProfile(int userId, string? email, string? password, string? fullname);
        public Task<bool> UpdateUserAvatar(int userId, string avatarFileName);
        public Task<bool> ExistUserNameAsync(string username);
        public Task<bool> UpdateUserNameAsync(string username,int userId);
        public Task<bool> UpdateUserPassword(string newPassword, int userId);
        public Task<string?> GetPasswordByUserId(int userId);
    }
}
