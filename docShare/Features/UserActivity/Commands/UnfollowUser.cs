using Application.Common;
using Application.Interfaces;

namespace API.Features.UserActivity.Commands;

public record UnfollowUserCommand(int FollowerId, int FollowedId);

public class UnfollowUserHandler
{
    private readonly IUserActivity _repo;

    public UnfollowUserHandler(IUserActivity repo)
    {
        _repo = repo;
    }

    public async Task<Result> HandleAsync(UnfollowUserCommand cmd, CancellationToken ct = default)
    {
        if (cmd.FollowerId == cmd.FollowedId)
            return Result.Failure("Không thể bỏ theo dõi chính mình.");

        bool isFollowing = await _repo.HasFollowedAsync(cmd.FollowerId, cmd.FollowedId);
        if (!isFollowing)
            return Result.Failure("Chưa theo dõi người dùng này.");

        await _repo.RemoveFollowingAsync(cmd.FollowerId, cmd.FollowedId);
        await _repo.SaveChangeAsync();
        return Result.Success();
    }
}
