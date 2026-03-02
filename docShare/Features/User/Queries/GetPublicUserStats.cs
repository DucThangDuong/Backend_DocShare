using Application.Common;
using Application.DTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Features.User.Queries;

public record GetPublicUserStatsQuery(int UserId);

public class GetPublicUserStatsHandler : IQueryHandler<GetPublicUserStatsQuery, ResUserStatsDto>
{
    private readonly DocShareContext _context;

    public GetPublicUserStatsHandler(DocShareContext context)
    {
        _context = context;
    }

    public async Task<Result<ResUserStatsDto>> HandleAsync(GetPublicUserStatsQuery query, CancellationToken ct = default)
    {
        bool exists = await _context.Users.AnyAsync(e => e.Id == query.UserId, ct);
        if (!exists)
            return Result<ResUserStatsDto>.Failure("Không xác định được danh tính người dùng.", 401);

        var stats = await _context.Users.AsNoTracking()
            .Where(u => u.Id == query.UserId)
            .Select(u => new ResUserStatsDto
            {
                UploadCount = _context.Documents.Count(d => d.UploaderId == query.UserId && d.IsDeleted == 0),
                SavedCount = _context.SavedDocuments.Count(s => s.UserId == query.UserId),
                TotalLikesReceived = _context.Documents.Where(e => e.UploaderId == query.UserId).Select(e => (int?)e.LikeCount).Sum() ?? 0
            })
            .FirstOrDefaultAsync(ct);

        return Result<ResUserStatsDto>.Success(stats ?? new ResUserStatsDto { SavedCount = 0, UploadCount = 0, TotalLikesReceived = 0 });
    }
}
