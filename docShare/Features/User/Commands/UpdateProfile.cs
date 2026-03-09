using Application.Common;
using Application.DTOs;
using Application.Interfaces;

namespace API.Features.User.Commands;

public record UpdateProfileCommand(int UserId, string? Email, string? Password, string? FullName, int? UniversityId);

public class UpdateProfileHandler
{
    private readonly IUsers _repo;

    public UpdateProfileHandler(IUsers repo)
    {
        _repo = repo;
    }

    public async Task<Result<ResUserPrivate?>> HandleAsync(UpdateProfileCommand cmd, CancellationToken ct = default)
    {
        if (!await _repo.ExistsAsync(cmd.UserId))
            return Result<ResUserPrivate?>.Failure("Token không hợp lệ hoặc thiếu thông tin định danh.", 401);

        await _repo.UpdateUserProfileAsync(cmd.UserId, cmd.Email, cmd.Password, cmd.FullName, cmd.UniversityId);
        await _repo.SaveChangesAsync();

        var updatedProfile = await _repo.GetUserPrivateProfileAsync(cmd.UserId);
        return Result<ResUserPrivate?>.Success(updatedProfile);
    }
}

