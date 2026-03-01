using Application.Interfaces;
using Domain.Entities;
using FastEndpoints;
using Microsoft.Extensions.Caching.Memory;

namespace API.Endpoints.Universities;

public class GetAllUniversitiesEndpoint : EndpointWithoutRequest
{
    public IUnitOfWork Repo { get; set; } = null!;
    public IMemoryCache Cache { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/universities");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string cacheKey = "universities";
        if (Cache.TryGetValue(cacheKey, out List<University>? result))
        {
            await Send.ResponseAsync(result!, 200, ct);
            return;
        }
        result = await Repo.universititesRepo.GetUniversityAsync();
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(2));
        Cache.Set(cacheKey, result, cacheOptions);
        await Send.ResponseAsync(result!, 200, ct);
    }
}
