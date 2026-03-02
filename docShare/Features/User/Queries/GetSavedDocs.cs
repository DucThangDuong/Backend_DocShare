using Application.Common;
using Application.DTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Features.User.Queries;

public record GetSavedDocsQuery(int UserId);

public class GetSavedDocsHandler : IQueryHandler<GetSavedDocsQuery, List<ResSummaryDocumentDto>?>
{
    private readonly DocShareContext _context;

    public GetSavedDocsHandler(DocShareContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ResSummaryDocumentDto>?>> HandleAsync(GetSavedDocsQuery query, CancellationToken ct = default)
    {
        var result = await _context.SavedDocuments
            .AsNoTracking()
            .Where(s => s.UserId == query.UserId)
            .Select(s => new ResSummaryDocumentDto
            {
                Id = s.Document.Id,
                Title = s.Document.Title,
                CreatedAt = s.Document.CreatedAt,
                Thumbnail = s.Document.Thumbnail,
                PageCount = s.Document.PageCount,
                LikeCount = s.Document.LikeCount
            }).ToListAsync(ct);

        return Result<List<ResSummaryDocumentDto>?>.Success(result);
    }
}
