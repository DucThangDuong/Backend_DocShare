using Application.Interfaces;
using Application.IServices;
using FastEndpoints;

namespace API.Endpoints.Auth;

public class RefreshTokenEndpoint : EndpointWithoutRequest
{
    public IUsers Repo { get; set; } = null!;
    public IJwtTokenService JwtTokenService { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/auth/refresh-token");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("token_refresh"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var refreshToken = HttpContext.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            await Send.ResponseAsync(new { message = "Không tìm thấy Refresh Token trong Cookie." }, 400, ct);
            return;
        }
        var user = await Repo.GetUserByRefreshTokenAsync(refreshToken);
        if (user == null)
        {
            await Send.ResponseAsync(new { message = "Token không hợp lệ." }, 401, ct);
            return;
        }
        if (user.RefreshToken != refreshToken)
        {
            await Send.ResponseAsync(new { message = "Token không khớp." }, 401, ct);
            return;
        }
        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            await Send.ResponseAsync(new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." }, 401, ct);
            return;
        }
        var newAccessToken = JwtTokenService.GenerateAccessToken(user.Id, user.Email!, user.Role!);
        await Send.ResponseAsync(new { success = true, accessToken = newAccessToken }, 200, ct);
    }
}
