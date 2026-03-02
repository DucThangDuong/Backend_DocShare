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
        public async Task<bool> HasEmailAsync(string email)
        {
            return await _context.Users.AsNoTracking().AnyAsync(e => e.Email == email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.Email == email);
        }



        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(e => e.RefreshToken == refreshToken);
        }

        public async Task DeleteRefreshTokenAsync(int userId)
        {
            User? userrevoke = await _context.Users.FirstOrDefaultAsync(e => e.Id == userId);
            if (userrevoke != null)
            {
                userrevoke.RefreshToken = null;
                userrevoke.RefreshTokenExpiryTime = null;
            }
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
                    fullname = u.FullName ?? string.Empty,
                    storagelimit = u.StorageLimit,
                    usedstorage = u.UsedStorage,
                    avatarUrl = u.LoginProvider == "Custom" ? u.CustomAvatar : u.GoogleAvatar,
                    UniversityId = u.UniversityId,
                    UniversityName = u.University != null ? u.University.Name : null,
                    hasPassword = !string.IsNullOrEmpty(u.PasswordHash),
                    FollowerCount = u.FollowingCount,
                    FollowingCount = u.FollowingCount
                }).FirstOrDefaultAsync();
        }

        public async Task UpdateUserProfile(int userId, string? email, string? password, string? fullname, int? universityId)
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
            if (universityId != null)
            {
                user!.UniversityId = universityId;
            }
        }
        public async Task UpdateUserAvatar(int userId, string avatarFileName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            user!.CustomAvatar = avatarFileName;
        }

        public async Task<bool> HasUserNameAsync(string username)
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

        public async Task<bool> HasValue(int userId)
        {
            return await _context.Users.AnyAsync(e => e.Id == userId);
        }

        public async Task CreateUserCustom(string email, string password, string fullname)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            string username = email.Substring(0, email.LastIndexOf('@'));
            User? newuser = await _context.Users.FirstOrDefaultAsync(e => e.Email == email);
            if (newuser != null)
            {
                newuser.PasswordHash = passwordHash;
                newuser.CustomAvatar = "default-avatar.jpg";
            }
            else
            {
                var newUser = new User
                {
                    Email = email,
                    PasswordHash = passwordHash,
                    Username = username,
                    FullName = fullname,
                    CreatedAt = DateTime.UtcNow,
                    Role = "User",
                    IsActive = true,
                    LoginProvider = "Custom"
                };
                _context.Users.Add(newUser);
            }
        }
        public async void CreateUser(User user)
        {
            _context.Users.Add(user);
        }



        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
