using API.Features.Tags.Queries;
using FastEndpoints;

namespace API.Endpoints.Tags;

public class GetTagsRequest
{
    public int Take { get; set; } = 10;
}

public class GetTagsEndpoint : Endpoint<GetTagsRequest>
{
    public GetTagHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/tags");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetTagsRequest req, CancellationToken ct)
    {
        var result = await Handler.HandleAsync(new GetTagsQuery(req.Take), ct);
        await Send.ResponseAsync(result.Data, 200, ct);
    }
}
