using API.DTOs;
using Application.Interfaces;
using Application.IServices;
using Azure.Core;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensibility;
using System.Security.Claims;
using API.Extensions;

namespace API.Controllers
{
    [Route("api")]
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
        [EnableRateLimiting("ip_login")]
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
            string role = userEntity.Role ?? "User";
            var accessToken = _generateJwtToken.GenerateAccessToken(userEntity.Id, userEntity.Email!, role);
            var refreshToken = _generateJwtToken.GenerateRefreshToken();

            await _repo.usersRepo.SaveRefreshTokenAsync(userEntity.Id, refreshToken.Token, refreshToken.ExpiryDate);
            SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiryDate);
            var userResponse = new
            {
                Id = userEntity.Id,
                Email = userEntity.Email,
                HoTen = userEntity.FullName,
                Role = role
            };
            return Ok(new
            {
                success = true,
                message = "Đăng nhập thành công",
                accessToken = accessToken,
                user = userResponse
            });
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
        [EnableRateLimiting("ip_login")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ReqRegisterDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bool isEmailExists = await _repo.usersRepo.ExistEmailAsync(request.Email);
            if (isEmailExists)
            {
                return Conflict (new { message = "Email này đã tồn tại" });
            }
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            string username= request.Email.Substring(0, request.Email.LastIndexOf('@'));
            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                Username = username,
                FullName = request.Fullname,
                CreatedAt = DateTime.Now,
                Role = "User",
                IsActive = true,
                AvatarUrl= "default-avatar.jpg",
            };
            bool result=await _repo.usersRepo.CreateUserAsync(newUser);
            if (!result)
            {
                return StatusCode(500, new
                {
                    message = "Loi khi them du lieu"
                });
            }
            return Created();
        }
        [HttpPost("google-login")]
        [EnableRateLimiting("ip_login")]
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
                    return Ok(new
                    {
                        accesstoken = result.CustomJwtToken
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
            Response.Cookies.Delete("refreshToken");
            int userId = User.GetUserId();
            if (userId != 0)
            {
                await _repo.usersRepo.RevokeRefreshTokenAsync(userId);
            }
            return Ok(new { message = "Đăng xuất thành công" });
        }
    }
}   
