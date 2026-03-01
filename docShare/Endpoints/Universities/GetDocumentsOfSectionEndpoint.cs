using Application.DTOs;
using Application.Interfaces;
using FastEndpoints;

namespace API.Endpoints.Universities;

public class GetDocumentsOfSectionRequest
{
    public int SectionId { get; set; }
}

public class GetDocumentsOfSectionEndpoint : Endpoint<GetDocumentsOfSectionRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/universities/sections/{sectionId}/documents");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetDocumentsOfSectionRequest req, CancellationToken ct)
    {
        bool ishas = await Repo.universititesRepo.HasUniSection(req.SectionId);
        if (!ishas) { await Send.ResponseAsync(null, 404, ct); return; }
        var result = await Repo.universititesRepo.GetDocOfSection(req.SectionId);
        await Send.ResponseAsync(result, 200, ct);
    }
}
