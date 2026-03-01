using Application.Common;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace API.Features.Documents.Commands;

public record MoveToTrashCommand(int UserId, int DocId, bool IsDeleted);

public class MoveToTrashHandler : ICommandHandler<MoveToTrashCommand>
{
    private readonly IDocuments _repo;
    private readonly IMemoryCache _cache;

    public MoveToTrashHandler(IDocuments repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<Result> HandleAsync(MoveToTrashCommand cmd, CancellationToken ct = default)
    {
        if (!await _repo.HasValue(cmd.DocId))
            return Result.Failure("Không tìm thấy tài liệu.", 404);
        if (!cmd.IsDeleted)
            return Result.Failure("Yêu cầu không hợp lệ.");

        await _repo.MoveToTrash(cmd.DocId);
        await _repo.SaveChangeAsync();

        if (cmd.UserId != 0)
        {
            _cache.Remove($"doc_detail_{cmd.DocId}_{cmd.UserId}");
            _cache.Remove($"user_stats_{cmd.UserId}");
        }
        return Result.Success();
    }
}
