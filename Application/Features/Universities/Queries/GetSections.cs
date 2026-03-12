using Application.Common;
using Domain.Entities;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Universities.Queries;

public record GetSectionsQuery(int UniversityId);

public class GetSectionsHandler : IQueryHandler<GetSectionsQuery, List<UniversitySection>?>
{
    private readonly IApplicationDbContext _context;

    public GetSectionsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UniversitySection>?>> HandleAsync(GetSectionsQuery query, CancellationToken ct = default)
    {
        bool exists = await _context.Universities.AsNoTracking().AnyAsync(e => e.Id == query.UniversityId, ct);
        if (!exists)
            return Result<List<UniversitySection>?>.Failure("KhÃ´ng tÃ¬m tháº¥y trÆ°á»ng Ä‘áº¡i há»c.", 404);

        var result = await _context.UniversitySections
            .AsNoTracking()
            .Where(e => e.UniversityId == query.UniversityId)
            .ToListAsync(ct);

        return Result<List<UniversitySection>?>.Success(result);
    }
}
