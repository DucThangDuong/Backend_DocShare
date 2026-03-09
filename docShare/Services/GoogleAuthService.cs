using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Google.Apis.Auth;

namespace API.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _repo;
        private readonly RabbitMQService _rabbitMQ;

        public GoogleAuthService(IConfiguration configuration, IJwtTokenService jwtTokenService, IUnitOfWork repo, RabbitMQService rabbitMQ)
        {
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
            _repo = repo;
            _rabbitMQ = rabbitMQ;
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
                return new AuthResultDTO { IsSuccess = false, ErrorMessage = "Token Google không h?p l? ho?c dã h?t h?n." };
            }
            try
            {


                string email = payload.Email;
                string name = payload.Name;
                string picture = payload.Picture;
                string googleId = payload.Subject;
                var user = await _repo.UsersRepo.GetByEmailAsync(email);
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

                    _repo.UsersRepo.CreateUser(user);
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
                await _repo.SaveChangesAsync();

                string customJwtToken = _jwtTokenService.GenerateAccessToken(user.Id, email, user.Role!);
                await _rabbitMQ.SendEmailResquest(new SendMailRequestDto
                {
                    Email = email,
                    Subject = "Ðang nh?p thành công vào DocShare",
                    HtmlMessage = $"Xin chào {name},\n\nB?n dã dang nh?p thành công vào DocShare b?ng tài kho?n Google c?a mình. " +
                    $"N?u b?n không ph?i là ngu?i dã th?c hi?n dang nh?p này, vui lòng liên h? v?i chúng tôi ngay l?p t?c.\n\nTrân tr?ng,\nÐ?i ngu DocShare"
                });
                return new AuthResultDTO
                {
                    IsSuccess = true,
                    CustomJwtToken = customJwtToken,
                    refreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDTO { IsSuccess = false, ErrorMessage = $"Ðã x?y ra l?i trong quá trình x? lý dang nh?p Google: {ex.Message}" };
            }
        }
    }
}

