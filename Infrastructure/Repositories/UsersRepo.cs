using Application.DTOs;
using Application.Interfaces;
using Azure.Core;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
namespace Infrastructure.Repositories
{
    public class UsersRepo : IUsers
    {
        DocShareContext _context;
        public UsersRepo(DocShareContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<bool> ExistEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(e => e.Email == email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.RefreshToken == refreshToken);
        }

        public async Task RevokeRefreshTokenAsync(int userId)
        {
            User? userrevoke = await _context.Users.FirstOrDefaultAsync(e => e.Id == userId);
            if (userrevoke != null)
            {
                userrevoke.RefreshToken = null;
                userrevoke.RefreshTokenExpiryTime = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id ==userId );

            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryDate;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<ResUserStorageFileDto?> GetUserStorageStatsAsync(int userId)
        {
            return await _context.Users
                .Where(e => e.Id == userId)
                .Select(u => new ResUserStorageFileDto
                {
                    StorageLimit = u.StorageLimit,
                    UsedStorage = u.UsedStorage,
                    TotalCount = u.Documents.Count(d => d.IsDeleted == 0),
                    Trash = u.Documents.Count(d => d.IsDeleted == 1)
                })
                .FirstOrDefaultAsync();
        }

        public Task<ResUserPrivate?> GetUserPrivateProfileAsync(int userId)
        {
            return _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new ResUserPrivate
                {
                    id = u.Id,
                    email = u.Email,
                    username = u.Username,
                    fullname = u.FullName,
                    createdat = u.CreatedAt,
                    storagelimit = u.StorageLimit,
                    usedstorage = u.UsedStorage,
                    avatarurl=u.AvatarUrl ?? ""
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateUserProfile(int userId, string? email, string? password, string? fullname)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                user.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }
            if (!string.IsNullOrWhiteSpace(fullname))
            {
                user.FullName = fullname;
            }
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateUserAvatar(int userId, string avatarFileName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }
            user.AvatarUrl = avatarFileName;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistUserNameAsync(string username)
        {
            return await _context.Users.AnyAsync(e=>e.Username == username);
        }

        public async Task<bool> UpdateUserNameAsync(string username, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Id == userId);
            if (user == null)
            {
                return false;
            }
            user.Username = username;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserPassword(string newPassword, int userId)
        {
            var user= await _context.Users.FirstOrDefaultAsync(e => e.Id == userId);
            if (user == null)
            {
                return false;
            }
            user.PasswordHash = newPassword;
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<string?> GetPasswordByUserId(int userId)
        {
            return _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.PasswordHash)
                .FirstOrDefaultAsync();
        }
    }
}
