using Application.Common;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace API.Features.Documents.Commands;

public record ClearDocumentFileCommand(int UserId, int DocId);

public class ClearDocumentFileHandler : ICommandHandler<ClearDocumentFileCommand>
{
    private readonly IDocuments _repo;
    private readonly IMemoryCache _cache;

    public ClearDocumentFileHandler(IDocuments repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<Result> HandleAsync(ClearDocumentFileCommand cmd, CancellationToken ct = default)
    {
        if (!await _repo.ExistsAsync(cmd.DocId))
            return Result.Failure("Không tìm thấy tài liệu", 404);

        await _repo.ClearFileContentUrlAsync(cmd.DocId);
        await _repo.SaveChangesAsync();
        _cache.Remove($"doc_detail_{cmd.DocId}_{cmd.UserId}");
        return Result.Success();
    }
}

