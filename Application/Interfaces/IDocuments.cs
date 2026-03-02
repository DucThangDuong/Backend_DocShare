using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IDocuments
    {
        public void Create(Document document);

        public Task<List<ResDocumentDetailEditDto>> GetDocsByUserIdPagedAsync(int userId, int skip, int take);
        public Task<Document?> GetDocByIDAsync(int docId);
        public Task<ResDocumentDetailDto?> GetDocByUserIDAsync(int docID,int? currentUserId);
        public Task<bool> HasValue(int docID);
        public Task MoveToTrash(int docID);
        public void Update(Document document);
        public Task ClearFileContentUrl(int docid);
        public Task<ResUserStatsDto?> GetUserStatsAsync(int userId);
        public Task<ResDocumentDetailEditDto?> GetDocumentDetailEditAsync(int userId, int docId);

        public Task SaveChangeAsync();

    }
}
