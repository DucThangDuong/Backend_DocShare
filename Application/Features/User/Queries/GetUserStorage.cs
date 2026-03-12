using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.User.Queries;

public record GetUserStorageQuery(int UserId);

public class GetUserStorageHandler : IQueryHandler<GetUserStorageQuery, ResUserStorageFileDto?>
{
    private readonly IApplicationDbContext _context;

    public GetUserStorageHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ResUserStorageFileDto?>> HandleAsync(GetUserStorageQuery query, CancellationToken ct = default)
    {
        var result = await _context.Users
            .Where(e => e.Id == query.UserId)
            .Select(u => new ResUserStorageFileDto
            {
                StorageLimit = u.StorageLimit,
                UsedStorage = u.UsedStorage,
                TotalCount = u.Documents.Count(d => d.IsDeleted == 0),
                Trash = u.Documents.Count(d => d.IsDeleted == 1)
            })
            .FirstOrDefaultAsync(ct);

        if (result == null)
            return Result<ResUserStorageFileDto?>.Failure("KhÃ´ng tÃ¬m tháº¥y thÃ´ng tin ngÆ°á»i dÃ¹ng.", 404);

        return Result<ResUserStorageFileDto?>.Success(result);
    }
}
