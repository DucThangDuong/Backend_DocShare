using API.Extensions;
using Application.Features.UserActivity.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.UserActivity;

public class UnfollowUserRequest
{
    public int UserId { get; set; }
}

public class UnfollowUserEndpoint : Endpoint<UnfollowUserRequest>
{
    public UnfollowUserHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Delete("/api/user-activity/unfollow/{userId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(UnfollowUserRequest req, CancellationToken ct)
    {
        int followerId = HttpContext.User.GetUserId();
        if (followerId == 0) { await Send.ResponseAsync(new { message = "KhÃ´ng xÃ¡c Ä‘á»‹nh Ä‘Æ°á»£c danh tÃ­nh ngÆ°á»i dÃ¹ng." }, 401, ct); return; }

        try
        {
            var result = await Handler.HandleAsync(new UnfollowUserCommand(followerId, req.UserId), ct);

            if (!result.IsSuccess)
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            else
                await Send.ResponseAsync(new { message = "ÄÃ£ bá» theo dÃµi ngÆ°á»i dÃ¹ng." }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = ex.Message }, 400, ct);
        }
    }
}
