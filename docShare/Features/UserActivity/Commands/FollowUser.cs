using Application.Common;
using Application.Interfaces;

namespace API.Features.UserActivity.Commands;

public record FollowUserCommand(int FollowerId, int FollowedId);

public class FollowUserHandler
{
    private readonly IUserActivity _repo;

    public FollowUserHandler(IUserActivity repo)
    {
        _repo = repo;
    }

    public async Task<Result> HandleAsync(FollowUserCommand cmd, CancellationToken ct = default)
    {
        if (cmd.FollowerId == cmd.FollowedId)
            return Result.Failure("Không thể theo dõi chính mình.");

        bool alreadyFollowed = await _repo.HasFollowedAsync(cmd.FollowerId, cmd.FollowedId);
        if (alreadyFollowed)
            return Result.Failure("Đã theo dõi người dùng này.");

        _repo.AddFollowing(cmd.FollowerId, cmd.FollowedId);
        await _repo.SaveChangeAsync();
        return Result.Success();
    }
}
