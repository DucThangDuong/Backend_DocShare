using Application.Common;
using Application.DTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Features.User.Queries;

public record GetUserDocumentsQuery(int UserId, int Skip, int Take);

public class GetUserDocumentsHandler : IQueryHandler<GetUserDocumentsQuery, List<ResDocumentDetailEditDto>>
{
    private readonly DocShareContext _context;

    public GetUserDocumentsHandler(DocShareContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ResDocumentDetailEditDto>>> HandleAsync(GetUserDocumentsQuery query, CancellationToken ct = default)
    {
        bool exists = await _context.Users.AnyAsync(e => e.Id == query.UserId, ct);
        if (!exists)
            return Result<List<ResDocumentDetailEditDto>>.Failure("Không xác định được danh tính người dùng.", 401);

        var take = query.Take > 50 ? 50 : query.Take;
        var result = await _context.Documents
            .AsNoTracking()
            .Where(d => d.UploaderId == query.UserId && d.IsDeleted == 0)
            .OrderByDescending(d => d.CreatedAt)
            .Skip(query.Skip).Take(take)
            .Select(d => new ResDocumentDetailEditDto
            {
                Id = d.Id,
                CreatedAt = d.CreatedAt,
                Description = d.Description,
                FileUrl = d.FileUrl,
                Title = d.Title,
                Thumbnail = d.Thumbnail,
                PageCount = d.PageCount,
                SizeInBytes = d.SizeInBytes,
                UpdatedAt = d.UpdatedAt,
                Status = d.Status,
                Tags = d.Tags.Select(e => e.Name).ToList(),
            }).ToListAsync(ct);

        return Result<List<ResDocumentDetailEditDto>>.Success(result);
    }
}
