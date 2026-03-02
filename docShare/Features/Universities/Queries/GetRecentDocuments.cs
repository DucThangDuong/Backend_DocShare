using Application.Common;
using Application.DTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace API.Features.Universities.Queries;

public record GetRecentDocumentsQuery(int UniversityId, int Skip, int Take);

public class GetRecentDocumentsHandler : IQueryHandler<GetRecentDocumentsQuery, List<ResSummaryDocumentDto>>
{
    private readonly DocShareContext _context;
    private readonly IMemoryCache _cache;

    public GetRecentDocumentsHandler(DocShareContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<List<ResSummaryDocumentDto>>> HandleAsync(GetRecentDocumentsQuery query, CancellationToken ct = default)
    {
        bool exists = await _context.Universities.AsNoTracking().AnyAsync(e => e.Id == query.UniversityId, ct);
        if (!exists)
            return Result<List<ResSummaryDocumentDto>>.Failure("Không tìm thấy trường đại học.", 404);

        string cacheKey = $"universities:{query.UniversityId}:documents:recent:{query.Skip}:{query.Take}";
        if (_cache.TryGetValue(cacheKey, out List<ResSummaryDocumentDto>? cached))
            return Result<List<ResSummaryDocumentDto>>.Success(cached!);

        var result = await _context.Universities.AsNoTracking()
            .Where(e => e.Id == query.UniversityId)
            .SelectMany(e => e.UniversitySections)
            .SelectMany(s => s.Documents)
            .OrderByDescending(d => d.CreatedAt)
            .Skip(query.Skip).Take(query.Take)
            .Select(d => new ResSummaryDocumentDto
            {
                Id = d.Id,
                CreatedAt = d.CreatedAt,
                Title = d.Title,
                LikeCount = d.LikeCount,
                Thumbnail = d.Thumbnail,
                PageCount = d.PageCount,
                Tags = d.Tags.Select(t => t.Name).ToList(),
            }).ToListAsync(ct);

        _cache.Set(cacheKey, result, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
        return Result<List<ResSummaryDocumentDto>>.Success(result);
    }
}
