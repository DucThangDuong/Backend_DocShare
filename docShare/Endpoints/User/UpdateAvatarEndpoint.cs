using Application.DTOs;
using API.Extensions;
using Application.Interfaces;
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
    public IUnitOfWork Repo { get; set; } = null!;
    public IStorageService S3Storage { get; set; } = null!;

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
        bool ishasuser = await Repo.usersRepo.HasValue(userId);
        if (userId == 0 || !ishasuser) { await Send.ResponseAsync(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." }, 401, ct); return; }
        try
        {
            if (req.Avatar != null && req.Avatar.Length > 0)
            {
                var ext = Path.GetExtension(req.Avatar.FileName);
                var avatarFileName = StringHelpers.Create_s3ObjectKey_avatar(ext, userId);
                using var stream = req.Avatar.OpenReadStream();
                await S3Storage.UploadFileAsync(stream, avatarFileName, req.Avatar.ContentType, StorageType.Avatar);
                await Repo.usersRepo.UpdateUserAvatar(userId, avatarFileName);
                await Repo.SaveAllAsync();
                await Send.ResponseAsync(new { data = avatarFileName }, 200, ct);
                return;
            }
            await Send.ResponseAsync(new { message = "Cập nhật thông tin người dùng thất bại." }, 400, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = $"Lỗi server: {ex.Message}" }, 500, ct);
        }
    }
}
