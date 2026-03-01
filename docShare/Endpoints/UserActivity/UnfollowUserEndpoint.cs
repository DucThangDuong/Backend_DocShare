using API.Extensions;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.UserActivity;

public class UnfollowUserRequest
{
    public int UserId { get; set; }
}

public class UnfollowUserEndpoint : Endpoint<UnfollowUserRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Delete("/api/user-activity/unfollow/{userId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(UnfollowUserRequest req, CancellationToken ct)
    {
        int followerId = HttpContext.User.GetUserId();
        if (followerId == 0) { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }
        if (followerId == req.UserId) { await Send.ResponseAsync(new { message = "Không thể bỏ theo dõi chính mình." }, 400, ct); return; }
        bool ishas = await Repo.userActivityRepo.HasFollowedAsync(followerId, req.UserId);
        if (!ishas) { await Send.ResponseAsync(new { message = "Chưa theo dõi người dùng này." }, 400, ct); return; }
        try
        {
            await Repo.userActivityRepo.RemoveFollowingAsync(followerId, req.UserId);
            await Repo.SaveAllAsync();
            await Send.ResponseAsync(new { message = "Đã bỏ theo dõi người dùng." }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = ex.Message }, 400, ct);
        }
    }
}
