using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace API.Features.Documents.Queries;

public record GetUserDocStatsQuery(int UserId);

public class GetUserDocStatsHandler : IQueryHandler<GetUserDocStatsQuery, ResUserStatsDto>
{
    private readonly IDocuments _repo;
    private readonly IMemoryCache _cache;

    public GetUserDocStatsHandler(IDocuments repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<Result<ResUserStatsDto>> HandleAsync(GetUserDocStatsQuery query, CancellationToken ct = default)
    {
        string cacheKey = $"user_stats_{query.UserId}";
        if (_cache.TryGetValue(cacheKey, out ResUserStatsDto? cached))
            return Result<ResUserStatsDto>.Success(cached!);

        var stats = await _repo.GetUserStatsAsync(query.UserId)
            ?? new ResUserStatsDto { SavedCount = 0, UploadCount = 0, TotalLikesReceived = 0 };

        _cache.Set(cacheKey, stats, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2)));
        return Result<ResUserStatsDto>.Success(stats);
    }
}
