using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;

namespace API.Features.User.Commands;

public record UpdateAvatarCommand(int UserId, string AvatarFileName, Stream AvatarStream, string ContentType);

public class UpdateAvatarHandler
{
    private readonly IUsers _repo;
    private readonly IStorageService _storageService;

    public UpdateAvatarHandler(IUsers repo, IStorageService storageService)
    {
        _repo = repo;
        _storageService = storageService;
    }

    public async Task<Result<string>> HandleAsync(UpdateAvatarCommand cmd, CancellationToken ct = default)
    {
        if (!await _repo.HasValue(cmd.UserId))
            return Result<string>.Failure("Token không hợp lệ hoặc thiếu thông tin định danh.", 401);

        await _storageService.UploadFileAsync(cmd.AvatarStream, cmd.AvatarFileName, cmd.ContentType, StorageType.Avatar);
        await _repo.UpdateUserAvatar(cmd.UserId, cmd.AvatarFileName);
        await _repo.SaveChangeAsync();
        return Result<string>.Success(cmd.AvatarFileName);
    }
}
