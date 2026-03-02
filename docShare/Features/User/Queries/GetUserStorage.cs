using Application.Common;
using Application.DTOs;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Features.User.Queries;

public record GetUserStorageQuery(int UserId);

public class GetUserStorageHandler : IQueryHandler<GetUserStorageQuery, ResUserStorageFileDto?>
{
    private readonly DocShareContext _context;

    public GetUserStorageHandler(DocShareContext context)
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
            return Result<ResUserStorageFileDto?>.Failure("Không tìm thấy thông tin người dùng.", 404);

        return Result<ResUserStorageFileDto?>.Success(result);
    }
}
