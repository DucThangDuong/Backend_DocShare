using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Google.Apis.Auth;
using System.Data;
using System.Security.Claims;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;

namespace API.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _repo;

        public GoogleAuthService(IConfiguration configuration, IJwtTokenService jwtTokenService, IUnitOfWork repo)
        {
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
            _repo = repo;
        }


        public async Task<AuthResultDTO> HandleGoogleLoginAsync(string idToken)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var googleClientId = _configuration["Authentication:Google:ClientId"];
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { googleClientId ?? "" }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException)
            {
                return new AuthResultDTO { IsSuccess = false, ErrorMessage = "Token Google không hợp lệ hoặc đã hết hạn." };
            }
            try
            {


                string email = payload.Email;
                string name = payload.Name;
                string picture = payload.Picture;
                string googleId = payload.Subject;
                var user = await _repo.usersRepo.GetByEmailAsync(email);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                if (user == null)
                {
                    user = new User
                    {
                        Email = email,
                        FullName = name,
                        GoogleAvatar = picture,
                        GoogleId = googleId,
                        Username = email.Split('@')[0],
                        PasswordHash = "",
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        RefreshToken = refreshToken.Token,
                        RefreshTokenExpiryTime = refreshToken.ExpiryDate,
                        LoginProvider = "Google"
                    };

                    _repo.usersRepo.CreateUser(user);
                }
                else
                {
                    if (string.IsNullOrEmpty(user.GoogleId))
                    {
                        user.GoogleId = googleId;
                    }
                    if (string.IsNullOrEmpty(user.GoogleAvatar))
                    {
                        user.GoogleAvatar = picture;
                    }
                    user.RefreshToken = refreshToken.Token;
                    user.RefreshTokenExpiryTime = refreshToken.ExpiryDate;
                    user.LoginProvider = "Google";
                }
                await _repo.SaveAllAsync();

                string customJwtToken = _jwtTokenService.GenerateAccessToken(user.Id, email, user.Role!);

                return new AuthResultDTO
                {
                    IsSuccess = true,
                    CustomJwtToken = customJwtToken,
                    refreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDTO { IsSuccess = false, ErrorMessage = $"Đã xảy ra lỗi trong quá trình xử lý đăng nhập Google: {ex.Message}" };
            }
        }
    }
}
