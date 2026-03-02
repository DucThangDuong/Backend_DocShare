using API.Features.Universities.Queries;
using FastEndpoints;

namespace API.Endpoints.Universities;

public class GetAllUniversitiesEndpoint : EndpointWithoutRequest
{
    public GetAllUniversitiesHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/universities");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await Handler.HandleAsync(new GetAllUniversitiesQuery(), ct);
        await Send.ResponseAsync(result.Data, 200, ct);
    }
}
