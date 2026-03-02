using API.DTOs;
using API.Extensions;
using API.Features.User.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class UpdateProfileEndpoint : Endpoint<ReqUserUpdateDto>
{
    public UpdateProfileHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Patch("/api/user/me/profile");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(ReqUserUpdateDto req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." }, 401, ct); return; }

        try
        {
            var result = await Handler.HandleAsync(new UpdateProfileCommand(userId, req.Email, req.Password, req.FullName, req.UniversityId), ct);

            if (!result.IsSuccess)
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            else
                await Send.ResponseAsync(result.Data!, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = $"Lỗi server: {ex.Message}" }, 500, ct);
        }
    }
}
