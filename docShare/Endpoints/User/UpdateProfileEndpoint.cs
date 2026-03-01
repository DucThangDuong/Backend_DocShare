using API.DTOs;
using API.Extensions;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class UpdateProfileEndpoint : Endpoint<ReqUserUpdateDto>
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Patch("/api/user/me/profile");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(ReqUserUpdateDto req, CancellationToken ct)
    {
        try
        {
            int userId = HttpContext.User.GetUserId();
            bool ishasuser = await Repo.usersRepo.HasValue(userId);
            if (userId == 0 || !ishasuser) { await Send.ResponseAsync(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." }, 401, ct); return; }
            await Repo.usersRepo.UpdateUserProfile(userId, req.Email, req.Password, req.FullName, req.UniversityId);
            await Repo.SaveAllAsync();
            var updatedProfile = await Repo.usersRepo.GetUserPrivateProfileAsync(userId);
            await Send.ResponseAsync(updatedProfile!, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = $"Lỗi server: {ex.Message}" }, 500, ct);
        }
    }
}
