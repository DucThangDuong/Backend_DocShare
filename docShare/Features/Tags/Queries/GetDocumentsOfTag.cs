using Amazon.Runtime.Internal.Util;
using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace API.Features.Tags.Queries;
public record GetDocumentsByTagQuery(int? Tagid, int Skip ,int Take );

public class GetDocumentsOfTagHandler : IQueryHandler<GetDocumentsByTagQuery, List<ResSummaryDocumentDto>>
    {
    private readonly DocShareContext _context;
    private readonly IMemoryCache _cache;
    public GetDocumentsOfTagHandler(DocShareContext context, IMemoryCache cache) { _context=context; _cache = cache; }

    public async Task<Result<List<ResSummaryDocumentDto>>> HandleAsync(GetDocumentsByTagQuery query, CancellationToken ct = default)
    {
        string cacheKey = $"tag_docs_{query.Tagid}_{query.Skip}_{query.Take}";
        var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);

            var query1 = _context.Documents.AsNoTracking();
            query1 = query1.Where(e => e.FileUrl != null && e.FileUrl != "");
            if (query.Tagid.HasValue && query.Tagid.Value > 0)
            {
                int id = query.Tagid.Value;
                query1 = query1.Where(d => d.Tags.Any(t => t.Id == id));
            }

            query1 = query1.OrderByDescending(d => d.CreatedAt).Skip(query.Skip).Take(query.Take);

            return await query1
                 .Select(d => new ResSummaryDocumentDto
                 {
                     Id = d.Id,
                     CreatedAt = d.CreatedAt,
                     Title = d.Title,
                     LikeCount = d.LikeCount,
                     Thumbnail = d.Thumbnail,
                     PageCount = d.PageCount,
                     Tags = d.Tags.Select(t => t.Name).ToList(),

                 }).ToListAsync();
        });
        return Result<List<ResSummaryDocumentDto>>.Success(result);
    }
}

