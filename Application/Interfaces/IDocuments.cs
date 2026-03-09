using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IDocuments
    {
        public void Add(Document document);

        public Task<List<ResDocumentDetailEditDto>> GetDocsByUserIdPagedAsync(int userId, int skip, int take);
        public Task<Document?> GetDocByIdAsync(int docId);
        public Task<ResDocumentDetailDto?> GetDocDetailByIdAsync(int docId,int? currentUserId);
        public Task<bool> ExistsAsync(int docId);
        public Task MoveToTrashAsync(int docId);
        public void Update(Document document);
        public Task ClearFileContentUrlAsync(int docId);
        public Task<ResUserStatsDto?> GetUserStatsAsync(int userId);
        public Task<ResDocumentDetailEditDto?> GetDocumentDetailEditAsync(int userId, int docId);

        public Task SaveChangesAsync();

    }
}
