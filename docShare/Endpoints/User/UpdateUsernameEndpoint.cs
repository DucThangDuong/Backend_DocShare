using API.DTOs;
using API.Extensions;
using API.Features.User.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class UpdateUsernameEndpoint : Endpoint<ReqUpdateUserNameDto>
{
    public UpdateUsernameHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Patch("/api/user/me/username");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(ReqUpdateUserNameDto req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.Username))
        {
            await Send.ResponseAsync(new { message = "Dữ liệu không hợp lệ." }, 400, ct);
            return;
        }

        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." }, 401, ct); return; }

        try
        {
            var result = await Handler.HandleAsync(new UpdateUsernameCommand(userId, req.Username), ct);

            if (!result.IsSuccess)
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            else
                await Send.ResponseAsync(new { data = result.Data }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = $"Lỗi server: {ex.Message}" }, 500, ct);
        }
    }
}
