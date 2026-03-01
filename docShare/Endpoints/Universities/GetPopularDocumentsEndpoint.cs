using Application.DTOs;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.Extensions.Caching.Memory;

namespace API.Endpoints.Universities;

public class GetPopularDocumentsRequest
{
    public int UniversityId { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
}

public class GetPopularDocumentsEndpoint : Endpoint<GetPopularDocumentsRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;
    public IMemoryCache Cache { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/universities/{universityId}/documents/popular");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetPopularDocumentsRequest req, CancellationToken ct)
    {
        bool ishas = await Repo.universititesRepo.HasValue(req.UniversityId);
        if (!ishas) { await Send.ResponseAsync(null, 404, ct); return; }
        string cacheKey = $"universities:{req.UniversityId}:documents:popular:{req.Skip}:{req.Take}";
        if (Cache.TryGetValue(cacheKey, out List<ResSummaryDocumentDto>? result))
        {
            await Send.ResponseAsync(result!, 200, ct); return;
        }
        result = await Repo.universititesRepo.GetPopularDocuments(req.UniversityId, req.Skip, req.Take);
        Cache.Set(cacheKey, result, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1)));
        await Send.ResponseAsync(result!, 200, ct);
    }
}
