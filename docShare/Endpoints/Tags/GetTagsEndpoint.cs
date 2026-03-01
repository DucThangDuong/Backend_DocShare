using Application.DTOs;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.Extensions.Caching.Memory;

namespace API.Endpoints.Tags;

public class GetTagsRequest
{
    public int Take { get; set; } = 10;
}

public class GetTagsEndpoint : Endpoint<GetTagsRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;
    public IMemoryCache Cache { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/tags");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetTagsRequest req, CancellationToken ct)
    {
        string cacheKey = "tags";
        if (!Cache.TryGetValue(cacheKey, out List<TagsDto>? tags))
        {
            tags = await Repo.tagsRepo.GetTags(req.Take);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));
            Cache.Set(cacheKey, tags, cacheOptions);
        }
        await Send.ResponseAsync(tags!, 200, ct);
    }
}
