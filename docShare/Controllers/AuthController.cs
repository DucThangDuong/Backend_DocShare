using API.DTOs;
using API.Extensions;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        private readonly IJwtTokenService _generateJwtToken;
        private readonly IGoogleAuthService _authService;
        public AuthController(IUnitOfWork repo , IJwtTokenService jwttoken, IGoogleAuthService authService)
        {
            _repo = repo;
            _generateJwtToken = jwttoken;
            _authService = authService;
        }
        //post
        [EnableRateLimiting("ip_auth")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ReqLoginDTo userlogin)
        {
            if (userlogin == null || string.IsNullOrEmpty(userlogin.Email) || string.IsNullOrEmpty(userlogin.Password))
            {
                return BadRequest(new { message = "Vui lòng nhập Email và Mật khẩu." });
            }
            User? userEntity = await _repo.usersRepo.GetByEmailAsync(userlogin.Email);
            if (userEntity == null || !BCrypt.Net.BCrypt.Verify(userlogin.Password, userEntity.PasswordHash))
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
            }
            try
            {
                string role = userEntity.Role ?? "User";
                var accessToken = _generateJwtToken.GenerateAccessToken(userEntity.Id, userEntity.Email!, role);
                var refreshToken = _generateJwtToken.GenerateRefreshToken();

                userEntity.RefreshToken = refreshToken.Token;
                userEntity.RefreshTokenExpiryTime = refreshToken.ExpiryDate;
                userEntity.LoginProvider = "Custom";
                SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiryDate);
                await _repo.SaveAllAsync();
                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công",
                    accessToken = accessToken,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        private void SetRefreshTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                Secure = true,
                SameSite = SameSiteMode.None,
                IsEssential = true
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
        [EnableRateLimiting("ip_auth")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ReqRegisterDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                bool isEmailExists = await _repo.usersRepo.ExistEmailAsync(request.Email);
                if (isEmailExists)
                {
                    return Conflict(new { message = "Email này đã tồn tại" });
                }
                await _repo.usersRepo.CreateUserCustomAsync(request.Email, request.Password, request.Fullname);
                await _repo.SaveAllAsync();
                return Created();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        [HttpPost("google")]
        [EnableRateLimiting("ip_auth")]
        public async Task<IActionResult> GoogleLogin([FromBody] ReqGoogleLoginDTO model)
        {
            if (string.IsNullOrEmpty(model.IdToken))
            {
                return BadRequest(new { message = "ID Token is required." });
            }
            try
            {
                var result = await _authService.HandleGoogleLoginAsync(model.IdToken);
                if (result.IsSuccess)
                {
                    SetRefreshTokenCookie(result.refreshToken.Token, result.refreshToken.ExpiryDate);
                    return Ok(new
                    {
                        accessToken = result.CustomJwtToken
                    });
                }
                else
                {
                    return Unauthorized(new { message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "Không tìm thấy Refresh Token trong Cookie." });
            }
            var user = await _repo.usersRepo.GetUserByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                return Unauthorized(new { message = "Token không hợp lệ." });
            }

            if (user.RefreshToken != refreshToken)
            {
                return Unauthorized(new { message = "Token không khớp." });
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }
            var newAccessToken = _generateJwtToken.GenerateAccessToken(user.Id, user.Email!, user.Role!);
            return Ok(new
            {
                success = true,
                accessToken = newAccessToken
            });
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                Response.Cookies.Delete("refreshToken");
                int userId = User.GetUserId();
                if (userId != 0)
                {
                    await _repo.usersRepo.RevokeRefreshTokenAsync(userId);
                    await _repo.SaveAllAsync();
                }
                return Ok(new { message = "Đăng xuất thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
    }
}   
