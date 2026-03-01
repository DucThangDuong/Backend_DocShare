using API.Extensions;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.UserActivity;

public class FollowUserRequest
{
    public int FollowedId { get; set; }
}

public class FollowUserEndpoint : Endpoint<FollowUserRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/user-activity/follow");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(FollowUserRequest req, CancellationToken ct)
    {
        int followerId = HttpContext.User.GetUserId();
        if (followerId == 0) { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }
        if (followerId == req.FollowedId) { await Send.ResponseAsync(new { message = "Không thể theo dõi chính mình." }, 400, ct); return; }
        bool ishas = await Repo.userActivityRepo.HasFollowedAsync(followerId, req.FollowedId);
        if (ishas) { await Send.ResponseAsync(new { message = "Đã theo dõi người dùng này." }, 400, ct); return; }
        try
        {
            Repo.userActivityRepo.AddFollowing(followerId, req.FollowedId);
            await Repo.SaveAllAsync();
            await Send.ResponseAsync(new { message = "Đã theo dõi người dùng." }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = ex.Message }, 400, ct);
        }
    }
}
