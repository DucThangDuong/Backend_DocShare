using API.Features.Tags.Queries;
using FastEndpoints;

namespace API.Endpoints.Tags;

public class GetDocumentsByTagRequest
{
    public int? Tagid { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class GetDocumentsByTagEndpoint : Endpoint<GetDocumentsByTagRequest>
{
    public GetDocumentsOfTagHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/tags/documents");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetDocumentsByTagRequest req, CancellationToken ct)
    {
        var result = await Handler.HandleAsync(new GetDocumentsByTagQuery(req.Tagid, req.Skip, req.Take), ct);
        await Send.ResponseAsync(result.Data, 200, ct);
    }
}
