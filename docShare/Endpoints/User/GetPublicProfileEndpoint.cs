using API.Extensions;
using API.Features.User.Queries;
using FastEndpoints;

namespace API.Endpoints.User;

public class GetPublicProfileRequest
{
    public int UserId { get; set; }
}

public class GetPublicProfileEndpoint : Endpoint<GetPublicProfileRequest>
{
    public GetPublicProfileHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/{userId}/profile");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(GetPublicProfileRequest req, CancellationToken ct)
    {
        int currentId = HttpContext.User.GetUserId();
        if (req.UserId <= 0) { await Send.ResponseAsync(new { message = "ID người dùng không hợp lệ." }, 400, ct); return; }

        var result = await Handler.HandleAsync(new GetPublicProfileQuery(req.UserId, currentId), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data, 200, ct);
    }
}
