using API.Extensions;
using API.Features.UserActivity.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.UserActivity;

public class FollowUserRequest
{
    public int FollowedId { get; set; }
}

public class FollowUserEndpoint : Endpoint<FollowUserRequest>
{
    public FollowUserHandler Handler { get; set; } = null!;

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

        try
        {
            var result = await Handler.HandleAsync(new FollowUserCommand(followerId, req.FollowedId), ct);

            if (!result.IsSuccess)
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            else
                await Send.ResponseAsync(new { message = "Đã theo dõi người dùng." }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = ex.Message }, 400, ct);
        }
    }
}
