using API.DTOs;
using API.Extensions;
using API.Features.User.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class UpdatePasswordEndpoint : Endpoint<ReqUpdatePasswordDto>
{
    public UpdatePasswordHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Patch("/api/user/me/password");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(ReqUpdatePasswordDto req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.NewPassword))
        {
            await Send.ResponseAsync(new { message = "Mật khẩu mới không được để trống." }, 400, ct);
            return;
        }

        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }

        try
        {
            var result = await Handler.HandleAsync(new UpdatePasswordCommand(userId, req.OldPassword, req.NewPassword), ct);

            if (!result.IsSuccess)
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            else
                await Send.NoContentAsync(ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = "Lỗi server nội bộ." }, 500, ct);
        }
    }
}
