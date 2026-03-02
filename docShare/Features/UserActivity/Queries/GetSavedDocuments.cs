using Application.Common;
using Domain.Entities;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace API.Features.UserActivity.Queries;

public record GetSavedDocumentsQuery(int UserId);

public class GetSavedDocumentsHandler : IQueryHandler<GetSavedDocumentsQuery, List<Document>>
{
    private readonly DocShareContext _context;

    public GetSavedDocumentsHandler(DocShareContext context)
    {
        _context = context;
    }

    public async Task<Result<List<Document>>> HandleAsync(GetSavedDocumentsQuery query, CancellationToken ct = default)
    {
        var docs = await _context.SavedDocuments
            .AsNoTracking()
            .Where(s => s.UserId == query.UserId)
            .Include(s => s.Document)
            .Select(s => s.Document)
            .ToListAsync(ct);

        return Result<List<Document>>.Success(docs);
    }
}
