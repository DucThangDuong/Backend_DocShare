using API.Extensions;
using API.Features.User.Commands;
using Application.IServices;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class UpdateAvatarRequest
{
    public IFormFile Avatar { get; set; } = null!;
}

public class UpdateAvatarEndpoint : Endpoint<UpdateAvatarRequest>
{
    public UpdateAvatarHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Patch("/api/user/me/avatar");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
        Options(x => x.RequireRateLimiting("write_heavy"));
    }

    public override async Task HandleAsync(UpdateAvatarRequest req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." }, 401, ct); return; }

        if (req.Avatar == null || req.Avatar.Length == 0)
        {
            await Send.ResponseAsync(new { message = "Cập nhật thông tin người dùng thất bại." }, 400, ct);
            return;
        }

        try
        {
            var ext = Path.GetExtension(req.Avatar.FileName);
            var avatarFileName = StringHelpers.Create_s3ObjectKey_avatar(ext, userId);
            using var stream = req.Avatar.OpenReadStream();
            var result = await Handler.HandleAsync(new UpdateAvatarCommand(userId, avatarFileName, stream, req.Avatar.ContentType), ct);

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
