using Application.Common;
using Application.DTOs;
using API.Extensions;
using Application.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Application.Features.User.Queries;

public record GetPublicProfileQuery(int UserId, int CurrentId);

public class GetPublicProfileHandler : IQueryHandler<GetPublicProfileQuery, ResUserPublicDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPublicProfileHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ResUserPublicDto?>> HandleAsync(GetPublicProfileQuery query, CancellationToken ct = default)
    {
        var result = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == query.UserId)
            .Select(u => new ResUserPublicDto
            {
                id = u.Id,
                username = u.Username,
                fullname = u.FullName ?? string.Empty,
                avatarUrl = u.LoginProvider == "Custom" ? u.CustomAvatar : u.GoogleAvatar,
                UniversityName = u.University != null ? u.University.Name : null,
                UniversityId = u.UniversityId,
                FollowerCount = u.FollowerCount,
                IsFollowing = u.UserFollowFolloweds.Any(f => f.FollowerId == query.CurrentId)
            }).FirstOrDefaultAsync(ct);

        if (result == null)
            return Result<ResUserPublicDto?>.Failure("KhÃ´ng tÃ¬m tháº¥y thÃ´ng tin ngÆ°á»i dÃ¹ng.", 404);

        result.avatarUrl = StringHelpers.GetFinalAvatarUrl(result.avatarUrl ?? "");
        return Result<ResUserPublicDto?>.Success(result);
    }
}
