using Application.Common;
using Application.Interfaces;

namespace API.Features.UserActivity.Commands;

public record VoteDocumentCommand(int UserId, int DocId, bool? IsLike);

public class VoteDocumentHandler
{
    private readonly IUserActivity _repo;

    public VoteDocumentHandler(IUserActivity repo)
    {
        _repo = repo;
    }

    public async Task<Result> HandleAsync(VoteDocumentCommand cmd, CancellationToken ct = default)
    {
        await _repo.ToggleDocumentVoteAsync(cmd.UserId, cmd.DocId, cmd.IsLike);
        await _repo.SaveChangesAsync();
        return Result.Success();
    }
}

