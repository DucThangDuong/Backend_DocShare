using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using API.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace API.Features.Documents.Queries;

public record GetDocumentDetailQuery(int DocId, int CurrentUserId);

public class GetDocumentDetailHandler : IQueryHandler<GetDocumentDetailQuery, ResDocumentDetailDto?>
{
    private readonly IDocuments _repo;
    private readonly IMemoryCache _cache;

    public GetDocumentDetailHandler(IDocuments repo, IMemoryCache cache) { _repo = repo; _cache = cache; }

    public async Task<Result<ResDocumentDetailDto?>> HandleAsync(GetDocumentDetailQuery query, CancellationToken ct = default)
    {
        string cacheKey = $"doc_detail_{query.DocId}";
        if (_cache.TryGetValue(cacheKey, out ResDocumentDetailDto? cached))
            return Result<ResDocumentDetailDto?>.Success(cached);

        if (!await _repo.HasValue(query.DocId))
            return Result<ResDocumentDetailDto?>.Failure("Không tìm thấy tài liệu.", 404);

        var result = await _repo.GetDocByUserIDAsync(query.DocId, query.CurrentUserId);
        if (result == null)
            return Result<ResDocumentDetailDto?>.Failure("Không tìm thấy tài liệu.", 404);

        result.AvatarUrl = StringHelpers.GetFinalAvatarUrl(result.AvatarUrl ?? "");
        return Result<ResDocumentDetailDto?>.Success(result);
    }
}
