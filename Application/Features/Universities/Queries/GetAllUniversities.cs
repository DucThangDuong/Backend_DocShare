using Application.Common;
using Domain.Entities;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Features.Universities.Queries;

public record GetAllUniversitiesQuery();

public class GetAllUniversitiesHandler : IQueryHandler<GetAllUniversitiesQuery, List<University>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public GetAllUniversitiesHandler(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<List<University>>> HandleAsync(GetAllUniversitiesQuery query, CancellationToken ct = default)
    {
        string cacheKey = "universities";
        if (_cache.TryGetValue(cacheKey, out List<University>? cached))
            return Result<List<University>>.Success(cached!);

        var result = await _context.Universities.AsNoTracking().ToListAsync(ct);
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(2));
        _cache.Set(cacheKey, result, cacheOptions);
        return Result<List<University>>.Success(result);
    }
}
