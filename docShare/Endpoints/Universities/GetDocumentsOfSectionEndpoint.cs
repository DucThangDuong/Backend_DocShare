using API.Features.Universities.Queries;
using FastEndpoints;

namespace API.Endpoints.Universities;

public class GetDocumentsOfSectionRequest
{
    public int SectionId { get; set; }
}

public class GetDocumentsOfSectionEndpoint : Endpoint<GetDocumentsOfSectionRequest>
{
    public GetDocumentsOfSectionHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/universities/sections/{sectionId}/documents");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetDocumentsOfSectionRequest req, CancellationToken ct)
    {
        var result = await Handler.HandleAsync(new GetDocumentsOfSectionQuery(req.SectionId), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data, 200, ct);
    }
}
