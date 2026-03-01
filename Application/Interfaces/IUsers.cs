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
        public Task<bool> HasEmailAsync(string email);
        public void CreateUser(User user);
        public Task<bool> HasValue(int userId);
        public Task<User?> GetByEmailAsync(string email);
        public Task<User?> GetUserAsync(int id);
        public Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        public Task DeleteRefreshTokenAsync(int userId);
        public Task<ResUserStorageFileDto?> GetUserStorageStatsAsync(int userId);
        public Task<ResUserPrivate?> GetUserPrivateProfileAsync(int userId);
        public Task UpdateUserProfile(int userId, string? email, string? password, string? fullname,int? universityId);
        public Task UpdateUserAvatar(int userId, string avatarFileName);
        public Task<bool> HasUserNameAsync(string username);
        public Task UpdateUserNameAsync(string username,int userId);
        public Task UpdateUserPassword(string newPassword, int userId);
        public Task<string?> GetPasswordByUserId(int userId);
        public Task CreateUserCustom(string email,string password,string fullname);
        public Task<ResUserPublicDto?> GetUserPublicProfileAsync(int userId, int currentId);
        public Task SaveChangeAsync();
    }
}
