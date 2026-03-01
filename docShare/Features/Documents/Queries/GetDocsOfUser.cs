using Application.Common;
using Application.DTOs;
using Application.Interfaces;

namespace API.Features.Documents.Queries;

public record GetDocsOfUserQuery(int UserId, int Skip, int Take);

public class GetDocsOfUserHandler : IQueryHandler<GetDocsOfUserQuery, List<ResDocumentDetailEditDto>>
{
    private readonly IDocuments _repo;

    public GetDocsOfUserHandler(IDocuments repo) { _repo = repo; }

    public async Task<Result<List<ResDocumentDetailEditDto>>> HandleAsync(GetDocsOfUserQuery query, CancellationToken ct = default)
    {
        var take = query.Take > 50 ? 50 : query.Take;
        var result = await _repo.GetDocsByUserIdPagedAsync(query.UserId, query.Skip, take);
        return Result<List<ResDocumentDetailEditDto>>.Success(result);
    }
}
