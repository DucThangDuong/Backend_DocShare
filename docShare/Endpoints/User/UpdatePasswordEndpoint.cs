using API.DTOs;
using API.Extensions;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class UpdatePasswordEndpoint : Endpoint<ReqUpdatePasswordDto>
{
    public IUnitOfWork Repo { get; set; } = null!;

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
        bool ishasuser = await Repo.usersRepo.HasValue(userId);
        if (userId == 0 || !ishasuser) { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }
        try
        {
            string? currentPasswordHash = await Repo.usersRepo.GetPasswordByUserId(userId);
            if (!string.IsNullOrEmpty(currentPasswordHash))
            {
                if (string.IsNullOrEmpty(req.OldPassword))
                {
                    await Send.ResponseAsync(new { message = "Vui lòng nhập mật khẩu cũ." }, 400, ct); return;
                }
                if (!BCrypt.Net.BCrypt.Verify(req.OldPassword, currentPasswordHash))
                {
                    await Send.ResponseAsync(new { message = "Mật khẩu cũ không chính xác." }, 400, ct); return;
                }
                if (req.OldPassword == req.NewPassword)
                {
                    await Send.ResponseAsync(new { message = "Mật khẩu mới không được trùng với mật khẩu cũ." }, 400, ct); return;
                }
            }
            await Repo.usersRepo.UpdateUserPassword(req.NewPassword, userId);
            await Repo.SaveAllAsync();
            await Send.NoContentAsync(ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = "Lỗi server nội bộ." }, 500, ct);
        }
    }
}
