using API.DTOs;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;

namespace API.Endpoints.Auth;

public class LoginEndpoint : Endpoint<ReqLoginDTo>
{
    public IUsers Repo { get; set; } = null!;
    public IJwtTokenService JwtTokenService { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth_strict"));
    }

    public override async Task HandleAsync(ReqLoginDTo req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password))
        {
            await Send.ResponseAsync(new { message = "Vui lòng nhập Email và Mật khẩu." }, 400, ct);
            return;
        }
        var userEntity = await Repo.GetByEmailAsync(req.Email);
        if (userEntity == null || !BCrypt.Net.BCrypt.Verify(req.Password, userEntity.PasswordHash))
        {
            await Send.ResponseAsync(new { message = "Tài khoản hoặc mật khẩu không chính xác." }, 401, ct);
            return;
        }
        try
        {
            string role = userEntity.Role ?? "User";
            var accessToken = JwtTokenService.GenerateAccessToken(userEntity.Id, userEntity.Email!, role);
            var refreshToken = JwtTokenService.GenerateRefreshToken();
            userEntity.RefreshToken = refreshToken.Token;
            userEntity.RefreshTokenExpiryTime = refreshToken.ExpiryDate;
            userEntity.LoginProvider = "Custom";
            HttpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true, Expires = refreshToken.ExpiryDate,
                Secure = true, SameSite = SameSiteMode.None, IsEssential = true
            });
            await Repo.SaveChangeAsync();
            await Send.ResponseAsync(new { success = true, message = "Đăng nhập thành công", accessToken }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = $"Lỗi server: {ex.Message}" }, 500, ct);
        }
    }
}
