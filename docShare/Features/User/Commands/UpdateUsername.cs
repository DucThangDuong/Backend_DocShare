using Application.Common;
using Application.Interfaces;

namespace API.Features.User.Commands;

public record UpdateUsernameCommand(int UserId, string Username);

public class UpdateUsernameHandler
{
    private readonly IUsers _repo;

    public UpdateUsernameHandler(IUsers repo)
    {
        _repo = repo;
    }

    public async Task<Result<string>> HandleAsync(UpdateUsernameCommand cmd, CancellationToken ct = default)
    {
        if (!await _repo.ExistsAsync(cmd.UserId))
            return Result<string>.Failure("Token không hợp lệ hoặc thiếu thông tin định danh.", 401);

        bool exists = await _repo.UsernameExistsAsync(cmd.Username);
        if (exists)
            return Result<string>.Failure("Username đã tồn tại.", 409);

        await _repo.UpdateUsernameAsync(cmd.UserId, cmd.Username);
        await _repo.SaveChangesAsync();
        return Result<string>.Success(cmd.Username);
    }
}

