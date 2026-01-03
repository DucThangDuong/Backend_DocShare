using API.DTOs;
using Application.Interfaces;
using Application.IServices;
using Azure.Core;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client.Extensibility;

namespace API.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        private readonly IJwtTokenService _generateJwtToken;
        public AuthController(IUnitOfWork repo , IJwtTokenService jwttoken)
        {
            _repo = repo;
            _generateJwtToken = jwttoken;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTo userlogin)
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
            string role = userEntity.Role ?? "Độc giả";
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
                SameSite = SameSiteMode.Strict,
                IsEssential = true
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bool isEmailExists = await _repo.usersRepo.ExistEmailAsync(request.Email);
            if (isEmailExists)
            {
                return BadRequest(new { message = "Email này đã được sử dụng." });
            }
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                Username = request.Username,
                FullName = request.Username,
                CreatedAt = DateTime.Now,
                Role = "Độc giả"
            };
            bool result=await _repo.usersRepo.CreateUserAsync(newUser);
            if (!result)
            {
                return StatusCode(500, new
                {
                    error = "Loi khi them du lieu"
                });
            }
            return Created();
        }
    }
}
