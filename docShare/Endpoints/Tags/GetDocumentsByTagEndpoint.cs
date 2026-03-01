using Application.DTOs;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.Extensions.Caching.Memory;

namespace API.Endpoints.Tags;

public class GetDocumentsByTagRequest
{
    public int? Tagid { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class GetDocumentsByTagEndpoint : Endpoint<GetDocumentsByTagRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;
    public IMemoryCache Cache { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/tags/documents");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetDocumentsByTagRequest req, CancellationToken ct)
    {
        string cacheKey = $"tag_docs_{req.Tagid}_{req.Skip}_{req.Take}";
        if (!Cache.TryGetValue(cacheKey, out List<ResSummaryDocumentDto>? result))
        {
            result = await Repo.tagsRepo.GetDocumentByTagID(req.Tagid, req.Skip, req.Take);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));
            Cache.Set(cacheKey, result, cacheOptions);
        }
        await Send.ResponseAsync(result!, 200, ct);
    }
}
