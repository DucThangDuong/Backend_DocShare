using Application.Common;
using Application.DTOs;
using Application.Interfaces;

namespace API.Features.Documents.Queries;

public record GetDocumentEditDetailQuery(int UserId, int DocId);

public class GetDocumentEditDetailHandler : IQueryHandler<GetDocumentEditDetailQuery, ResDocumentDetailEditDto?>
{
    private readonly IDocuments _repo;

    public GetDocumentEditDetailHandler(IDocuments repo) { _repo = repo; }

    public async Task<Result<ResDocumentDetailEditDto?>> HandleAsync(GetDocumentEditDetailQuery query, CancellationToken ct = default)
    {
        if (!await _repo.HasValue(query.DocId))
            return Result<ResDocumentDetailEditDto?>.Failure("Không tìm thấy tài liệu.", 404);

        var result = await _repo.GetDocumentDetailEditAsync(query.UserId, query.DocId);
        if (result == null)
            return Result<ResDocumentDetailEditDto?>.Failure("Không tìm thấy tài liệu.", 404);

        return Result<ResDocumentDetailEditDto?>.Success(result);
    }
}
