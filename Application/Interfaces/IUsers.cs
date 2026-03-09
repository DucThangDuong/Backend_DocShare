using Application.DTOs;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface IUsers
    {
        public Task<bool> EmailExistsAsync(string email);
        public void CreateUser(User user);
        public Task<bool> ExistsAsync(int userId);
        public Task<User?> GetByEmailAsync(string email);

        public Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        public Task RevokeRefreshTokenAsync(int userId);

        public Task<ResUserPrivate?> GetUserPrivateProfileAsync(int userId);
        public Task UpdateUserProfileAsync(int userId, string? email, string? password, string? fullname,int? universityId);
        public Task UpdateUserAvatarAsync(int userId, string avatarFileName);
        public Task<bool> UsernameExistsAsync(string username);
        public Task UpdateUsernameAsync(int userId, string username);
        public Task UpdatePasswordAsync(int userId, string newPassword);
        public Task<string?> GetPasswordHashAsync(int userId);
        public Task RegisterLocalUserAsync(string email,string password,string fullname);
        public Task SaveChangesAsync();
    }
}
