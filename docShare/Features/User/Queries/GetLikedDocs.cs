using Application.Common;
using Application.DTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Features.User.Queries;

public record GetLikedDocsQuery(int UserId);

public class GetLikedDocsHandler : IQueryHandler<GetLikedDocsQuery, List<ResSummaryDocumentDto>?>
{
    private readonly DocShareContext _context;

    public GetLikedDocsHandler(DocShareContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ResSummaryDocumentDto>?>> HandleAsync(GetLikedDocsQuery query, CancellationToken ct = default)
    {
        var result = await _context.DocumentVotes
            .AsNoTracking()
            .Where(v => v.UserId == query.UserId && v.IsLike)
            .Select(v => new ResSummaryDocumentDto
            {
                Id = v.Document.Id,
                Title = v.Document.Title,
                CreatedAt = v.Document.CreatedAt,
                Thumbnail = v.Document.Thumbnail,
                PageCount = v.Document.PageCount,
                LikeCount = v.Document.LikeCount
            }).ToListAsync(ct);

        return Result<List<ResSummaryDocumentDto>?>.Success(result);
    }
}
