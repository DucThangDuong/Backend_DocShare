using Application.Interfaces;
using FastEndpoints;

namespace API.Endpoints.Universities;

public class GetSectionsRequest
{
    public int UniversityId { get; set; }
}

public class GetSectionsEndpoint : Endpoint<GetSectionsRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/universities/{universityId}/sections");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetSectionsRequest req, CancellationToken ct)
    {
        bool ishas = await Repo.universititesRepo.HasValue(req.UniversityId);
        if (!ishas) { await Send.ResponseAsync(null, 404, ct); return; }
        var result = await Repo.universititesRepo.GetUniversitySectionsAsync(req.UniversityId);
        await Send.ResponseAsync(result!, 200, ct);
    }
}
