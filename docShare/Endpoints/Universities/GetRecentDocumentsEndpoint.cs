using API.Features.Universities.Queries;
using FastEndpoints;

namespace API.Endpoints.Universities;

public class GetRecentDocumentsRequest
{
    public int UniversityId { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
}

public class GetRecentDocumentsEndpoint : Endpoint<GetRecentDocumentsRequest>
{
    public GetRecentDocumentsHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/universities/{universityId}/documents/recent");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetRecentDocumentsRequest req, CancellationToken ct)
    {
        var result = await Handler.HandleAsync(new GetRecentDocumentsQuery(req.UniversityId, req.Skip, req.Take), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data, 200, ct);
    }
}
