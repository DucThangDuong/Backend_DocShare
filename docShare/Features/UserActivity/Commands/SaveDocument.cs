using Application.Common;
using Application.Interfaces;

namespace API.Features.UserActivity.Commands;

public record SaveDocumentCommand(int UserId, int DocId);

public class SaveDocumentHandler
{
    private readonly IUserActivity _repo;

    public SaveDocumentHandler(IUserActivity repo)
    {
        _repo = repo;
    }

    public async Task<Result> HandleAsync(SaveDocumentCommand cmd, CancellationToken ct = default)
    {
        await _repo.AddUserSaveDocumentAsync(cmd.UserId, cmd.DocId);
        await _repo.SaveChangeAsync();
        return Result.Success();
    }
}
