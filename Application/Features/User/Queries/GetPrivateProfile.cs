using Application.Common;
using Application.DTOs;
using API.Extensions;
using Application.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.User.Queries;

public record GetPrivateProfileQuery(int UserId);

public class GetPrivateProfileHandler : IQueryHandler<GetPrivateProfileQuery, ResUserPrivate?>
{
    private readonly IApplicationDbContext _context;

    public GetPrivateProfileHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ResUserPrivate?>> HandleAsync(GetPrivateProfileQuery query, CancellationToken ct = default)
    {
        var result = await _context.Users
            .Where(u => u.Id == query.UserId)
            .Select(u => new ResUserPrivate
            {
                id = u.Id,
                email = u.Email,
                username = u.Username,
                fullname = u.FullName ?? string.Empty,
                storagelimit = u.StorageLimit,
                usedstorage = u.UsedStorage,
                avatarUrl = u.LoginProvider == "Custom" ? u.CustomAvatar : u.GoogleAvatar,
                UniversityId = u.UniversityId,
                UniversityName = u.University != null ? u.University.Name : null,
                hasPassword = !string.IsNullOrEmpty(u.PasswordHash),
                FollowerCount = u.FollowingCount,
                FollowingCount = u.FollowingCount
            }).FirstOrDefaultAsync(ct);

        if (result == null)
            return Result<ResUserPrivate?>.Failure("KhÃ´ng tÃ¬m tháº¥y thÃ´ng tin ngÆ°á»i dÃ¹ng.", 404);

        result.avatarUrl = StringHelpers.GetFinalAvatarUrl(result.avatarUrl ?? "");
        return Result<ResUserPrivate?>.Success(result);
    }
}
