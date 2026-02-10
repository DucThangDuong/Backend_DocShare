using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UsersRepo : IUsers
    {
        private readonly DocShareContext _context;
        public UsersRepo(DocShareContext context)
        {
            _context = context;
        }

        public async Task CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<bool> ExistEmailAsync(string email)
        {
            return await _context.Users.AsNoTracking().AnyAsync(e => e.Email == email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(e => e.RefreshToken == refreshToken);
        }

        public async Task RevokeRefreshTokenAsync(int userId)
        {
            User? userrevoke = await _context.Users.FirstOrDefaultAsync(e => e.Id == userId);
            if (userrevoke != null)
            {
                userrevoke.RefreshToken = null;
                userrevoke.RefreshTokenExpiryTime = null;
            }
        }

        public async Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id ==userId );

            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryDate;
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

        public async Task UpdateUserProfile(int userId, string? email, string? password, string? fullname)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (!string.IsNullOrWhiteSpace(email))
            {
                user!.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                user!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }
            if (!string.IsNullOrWhiteSpace(fullname))
            {
                user!.FullName = fullname;
            }
        }
        public async Task UpdateUserAvatar(int userId, string avatarFileName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            user!.AvatarUrl = avatarFileName;
        }

        public async Task<bool> ExistUserNameAsync(string username)
        {
            return await _context.Users.AsNoTracking().AnyAsync(e => e.Username == username);
        }

        public async Task UpdateUserNameAsync(string username, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Id == userId);
            user!.Username = username;
        }

        public async Task UpdateUserPassword(string newPassword, int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Id == userId);
            user!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        }

        public Task<string?> GetPasswordByUserId(int userId)
        {
            return _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.PasswordHash)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasUser(int userId)
        {
            return await _context.Users.AnyAsync(e => e.Id == userId);
        }
    }
}
