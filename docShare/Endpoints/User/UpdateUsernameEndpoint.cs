using API.DTOs;
using API.Extensions;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class UpdateUsernameEndpoint : Endpoint<ReqUpdateUserNameDto>
{
    public IUnitOfWork Repo { get; set; } = null!;

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
        bool ishasuser = await Repo.usersRepo.HasValue(userId);
        if (userId == 0 || !ishasuser) { await Send.ResponseAsync(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." }, 401, ct); return; }
        try
        {
            bool ishasname = await Repo.usersRepo.HasUserNameAsync(req.Username);
            if (ishasname) { await Send.ResponseAsync(new { message = "Username đã tồn tại." }, 409, ct); return; }
            await Repo.usersRepo.UpdateUserNameAsync(req.Username, userId);
            await Repo.SaveAllAsync();
            await Send.ResponseAsync(new { data = req.Username }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = $"Lỗi server: {ex.Message}" }, 500, ct);
        }
    }
}
