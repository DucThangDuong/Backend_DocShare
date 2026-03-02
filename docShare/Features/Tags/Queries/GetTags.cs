using Amazon.Runtime.Internal.Util;
using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace API.Features.Tags.Queries;
 public record GetTagsQuery(int Take );
public class GetTagHandler : IQueryHandler<GetTagsQuery, List<TagsDto>>
{
    public readonly DocShareContext _context;
    public readonly IMemoryCache _cache;
    public GetTagHandler(DocShareContext context, IMemoryCache cache ) { _context=context; _cache = cache; }

    public async Task<Result<List<TagsDto>>> HandleAsync(GetTagsQuery query, CancellationToken ct = default)
    {
        string cacheKey = $"tags_take_{query.Take}";
        var tags = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            return await _context.Tags
                .AsNoTracking()
                .OrderByDescending(t => t.Documents.Count)
                .Take(query.Take)
                .Select(t => new TagsDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    count = t.Documents.Count
                })
                .ToListAsync();
        });
        return Result<List<TagsDto>>.Success(tags ?? new List<TagsDto>());
    }
}
