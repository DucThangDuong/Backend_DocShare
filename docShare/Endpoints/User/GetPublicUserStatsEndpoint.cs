using API.Features.User.Queries;
using FastEndpoints;

namespace API.Endpoints.User;

public class GetPublicUserStatsRequest
{
    public int UserId { get; set; }
}

public class GetPublicUserStatsEndpoint : Endpoint<GetPublicUserStatsRequest>
{
    public GetPublicUserStatsHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/{userId}/stats");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(GetPublicUserStatsRequest req, CancellationToken ct)
    {
        var result = await Handler.HandleAsync(new GetPublicUserStatsQuery(req.UserId), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data, 200, ct);
    }
}
