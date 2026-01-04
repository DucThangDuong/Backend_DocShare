using Application.IServices;
using Application.DTOs;
using System.Security.Claims;
using Google.Apis.Auth;

namespace API.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenService _jwtTokenService;

        public GoogleAuthService(IConfiguration configuration, IJwtTokenService jwtTokenService)
        {
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResultDTO> HandleGoogleLoginAsync(string googleIdToken)
        {
            var googleClientId = _configuration["Authentication:Google:ClientId"];

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId } 
                });
                var userEmail = payload.Email;
                var userName = payload.GivenName;
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, payload.Subject),
                    new Claim(ClaimTypes.Email, userEmail),
                    new Claim(ClaimTypes.Role, "User")
                };
                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Google"));
                var customJwtToken = _jwtTokenService.GenerateToken(principal);

                return new AuthResultDTO { IsSuccess = true, CustomJwtToken = customJwtToken };
            }
            catch (InvalidJwtException )
            {
                return new AuthResultDTO { IsSuccess = false, ErrorMessage = "Google Token is invalid or expired." };
            }
            catch (Exception )
            {
                return new AuthResultDTO { IsSuccess = false, ErrorMessage = "Server error during login process." };
            }
        }
    }
}
