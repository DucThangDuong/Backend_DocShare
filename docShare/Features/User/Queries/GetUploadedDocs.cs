using Application.Common;
using Application.DTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Features.User.Queries;

public record GetUploadedDocsQuery(int UserId);

public class GetUploadedDocsHandler : IQueryHandler<GetUploadedDocsQuery, List<ResSummaryDocumentDto>?>
{
    private readonly DocShareContext _context;

    public GetUploadedDocsHandler(DocShareContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ResSummaryDocumentDto>?>> HandleAsync(GetUploadedDocsQuery query, CancellationToken ct = default)
    {
        var result = await _context.Documents
            .AsNoTracking()
            .Where(d => d.UploaderId == query.UserId && d.IsDeleted == 0)
            .Select(d => new ResSummaryDocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                CreatedAt = d.CreatedAt,
                Thumbnail = d.Thumbnail,
                PageCount = d.PageCount,
                LikeCount = d.LikeCount
            }).ToListAsync(ct);

        return Result<List<ResSummaryDocumentDto>?>.Success(result);
    }
}
