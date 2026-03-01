using API.Extensions;
using Application.DTOs;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class GetPrivateProfileEndpoint : EndpointWithoutRequest
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/me/profile");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." }, 401, ct); return; }
        var result = await Repo.usersRepo.GetUserPrivateProfileAsync(userId);
        if (result == null) { await Send.ResponseAsync(new { message = "Không tìm thấy thông tin người dùng." }, 404, ct); return; }
        result.avatarUrl = StringHelpers.GetFinalAvatarUrl(result.avatarUrl ?? "");
        await Send.ResponseAsync(result, 200, ct);
    }
}
