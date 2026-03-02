using Application.Common;
using Application.Interfaces;

namespace API.Features.User.Commands;

public record UpdatePasswordCommand(int UserId, string? OldPassword, string NewPassword);

public class UpdatePasswordHandler
{
    private readonly IUsers _repo;

    public UpdatePasswordHandler(IUsers repo)
    {
        _repo = repo;
    }

    public async Task<Result> HandleAsync(UpdatePasswordCommand cmd, CancellationToken ct = default)
    {
        if (!await _repo.HasValue(cmd.UserId))
            return Result.Failure("Không xác định được danh tính người dùng.", 401);

        string? currentPasswordHash = await _repo.GetPasswordByUserId(cmd.UserId);
        if (!string.IsNullOrEmpty(currentPasswordHash))
        {
            if (string.IsNullOrEmpty(cmd.OldPassword))
                return Result.Failure("Vui lòng nhập mật khẩu cũ.");

            if (!BCrypt.Net.BCrypt.Verify(cmd.OldPassword, currentPasswordHash))
                return Result.Failure("Mật khẩu cũ không chính xác.");

            if (cmd.OldPassword == cmd.NewPassword)
                return Result.Failure("Mật khẩu mới không được trùng với mật khẩu cũ.");
        }

        await _repo.UpdateUserPassword(cmd.NewPassword, cmd.UserId);
        await _repo.SaveChangeAsync();
        return Result.Success();
    }
}
