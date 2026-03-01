using API.Extensions;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Auth;

public class LogoutEndpoint : EndpointWithoutRequest
{
    public IUsers Repo { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/auth/logout");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            HttpContext.Response.Cookies.Delete("refreshToken");
            int userId = HttpContext.User.GetUserId();
            if (userId != 0)
            {
                await Repo.DeleteRefreshTokenAsync(userId);
                await Repo.SaveChangeAsync();
            }
            await Send.ResponseAsync(new { message = "Đăng xuất thành công" }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = $"Lỗi server: {ex.Message}" }, 500, ct);
        }
    }
}
