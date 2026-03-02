using Application.DTOs;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface IUsers
    {
        public Task<bool> HasEmailAsync(string email);
        public void CreateUser(User user);
        public Task<bool> HasValue(int userId);
        public Task<User?> GetByEmailAsync(string email);

        public Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        public Task DeleteRefreshTokenAsync(int userId);

        public Task<ResUserPrivate?> GetUserPrivateProfileAsync(int userId);
        public Task UpdateUserProfile(int userId, string? email, string? password, string? fullname,int? universityId);
        public Task UpdateUserAvatar(int userId, string avatarFileName);
        public Task<bool> HasUserNameAsync(string username);
        public Task UpdateUserNameAsync(string username,int userId);
        public Task UpdateUserPassword(string newPassword, int userId);
        public Task<string?> GetPasswordByUserId(int userId);
        public Task CreateUserCustom(string email,string password,string fullname);

        public Task SaveChangeAsync();
    }
}
