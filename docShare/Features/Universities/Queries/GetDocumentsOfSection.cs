using Application.Common;
using Application.DTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Universities.Queries;

public record GetDocumentsOfSectionQuery(int SectionId);

public class GetDocumentsOfSectionHandler : IQueryHandler<GetDocumentsOfSectionQuery, List<ResSummaryDocumentDto>>
{
    private readonly DocShareContext _context;

    public GetDocumentsOfSectionHandler(DocShareContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ResSummaryDocumentDto>>> HandleAsync(GetDocumentsOfSectionQuery query, CancellationToken ct = default)
    {
        bool exists = await _context.UniversitySections.AsNoTracking().AnyAsync(e => e.Id == query.SectionId, ct);
        if (!exists)
            return Result<List<ResSummaryDocumentDto>>.Failure("Không tìm thấy khoa/ngành.", 404);

        var result = await _context.UniversitySections.AsNoTracking()
            .Where(e => e.Id == query.SectionId)
            .SelectMany(e => e.Documents)
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

        return Result<List<ResSummaryDocumentDto>>.Success(result);
    }
}
