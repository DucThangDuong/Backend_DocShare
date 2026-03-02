using Application.Common;
using Domain.Entities;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Universities.Queries;

public record GetSectionsQuery(int UniversityId);

public class GetSectionsHandler : IQueryHandler<GetSectionsQuery, List<UniversitySection>?>
{
    private readonly DocShareContext _context;

    public GetSectionsHandler(DocShareContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UniversitySection>?>> HandleAsync(GetSectionsQuery query, CancellationToken ct = default)
    {
        bool exists = await _context.Universities.AsNoTracking().AnyAsync(e => e.Id == query.UniversityId, ct);
        if (!exists)
            return Result<List<UniversitySection>?>.Failure("Không tìm thấy trường đại học.", 404);

        var result = await _context.UniversitySections
            .AsNoTracking()
            .Where(e => e.UniversityId == query.UniversityId)
            .ToListAsync(ct);

        return Result<List<UniversitySection>?>.Success(result);
    }
}
