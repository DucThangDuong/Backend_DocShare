using API.DTOs;
using Application.Interfaces;
using Application.IServices;
using Azure.Core;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensibility;
using System.Security.Claims;

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
                return Conflict (new { message = "Email này đã tồn tại" });
            }
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                Username = request.Username,
                FullName = request.Username,
                CreatedAt = DateTime.Now,
                Role = "User",
                IsActivate=1
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

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {

                var user = await _repo.usersRepo.GetUserAsync(int.Parse(userId));

                if (user == null) return NotFound();
                return Ok(new
                {
                    id = user.Id,
                    username = user.FullName,
                    email = user.Email,
                    avatar = user.AvartarUrl
                });
            }
            return BadRequest();
        }
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO model)
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
    }
}
