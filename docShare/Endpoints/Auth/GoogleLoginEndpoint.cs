using API.DTOs;
using Application.IServices;
using FastEndpoints;

namespace API.Endpoints.Auth;

public class GoogleLoginEndpoint : Endpoint<ReqGoogleLoginDTO>
{
    public IGoogleAuthService AuthService { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/auth/google");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth_strict"));
    }

    public override async Task HandleAsync(ReqGoogleLoginDTO req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.IdToken))
        {
            await Send.ErrorsAsync();
            return;
        }
        try
        {
            var result = await AuthService.HandleGoogleLoginAsync(req.IdToken);
            if (result.IsSuccess)
            {
                HttpContext.Response.Cookies.Append("refreshToken", result.refreshToken.Token, new CookieOptions
                {
                    HttpOnly = true, Expires = result.refreshToken.ExpiryDate,
                    Secure = true, SameSite = SameSiteMode.None, IsEssential = true
                });
                await Send.OkAsync(new { accessToken = result.CustomJwtToken }, ct);
            }
            else
            {
                await Send.ResponseAsync(new { message = result.ErrorMessage }, 401, ct);
            }
        }
        catch
        {
            await Send.ResponseAsync(new { message = "An unexpected error occurred." }, 500, ct);
        }
    }
}
