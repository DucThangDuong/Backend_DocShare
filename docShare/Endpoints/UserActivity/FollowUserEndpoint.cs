using API.Extensions;
using Application.Features.UserActivity.Commands;
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
        if (followerId == 0) { await Send.ResponseAsync(new { message = "KhÃ´ng xÃ¡c Ä‘á»‹nh Ä‘Æ°á»£c danh tÃ­nh ngÆ°á»i dÃ¹ng." }, 401, ct); return; }

        try
        {
            var result = await Handler.HandleAsync(new FollowUserCommand(followerId, req.FollowedId), ct);

            if (!result.IsSuccess)
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            else
                await Send.ResponseAsync(new { message = "ÄÃ£ theo dÃµi ngÆ°á»i dÃ¹ng." }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = ex.Message }, 400, ct);
        }
    }
}
