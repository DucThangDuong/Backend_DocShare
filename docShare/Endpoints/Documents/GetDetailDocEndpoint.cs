using API.Extensions;
using API.Features.Documents.Queries;
using FastEndpoints;

namespace API.Endpoints.Documents;

public class GetDetailDocRequest
{
    public int Docid { get; set; }
}

public class GetDetailDocEndpoint : Endpoint<GetDetailDocRequest>
{
    public GetDocumentDetailHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/documents/{docid}");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetDetailDocRequest req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        var result = await Handler.HandleAsync(new GetDocumentDetailQuery(req.Docid, userId), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data, 200, ct);
    }
}
