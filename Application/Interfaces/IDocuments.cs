using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IDocuments
    {
        public void Create(Document document);
        public Task<int> CountDocByUserID(int UserID);
        public Task<int> CountTrashByUserID(int UserID);
        public Task<List<ResDocumentDetailEditDto>> GetDocsByUserIdPagedAsync(int userId, int skip, int take);
        public Task<Document?> GetDocByIDAsync(int docId);
        public Task<ResDocumentDetailDto?> GetDocByUserIDAsync(int docID,int? currentUserId);
        public Task<bool> HasValue(int docID);
        public Task MoveToTrash(int docID);
        public void Update(Document document);
        public Task ClearFileContentUrl(int docid);
        public Task<ResUserStatsDto?> GetUserStatsAsync(int userId);
        public Task<ResDocumentDetailEditDto?> GetDocumentDetailEditAsync(int userId, int docId);
        public Task<List<ResSummaryDocumentDto>>? GetDocumentSaveOfUser(int userId);
        public Task<List<ResSummaryDocumentDto>>? GetDocumentLikeOfUser(int userId);
        public Task<List<ResSummaryDocumentDto>>? GetDocumentUploadOfUser(int userId);
        public Task SaveChangeAsync();

    }
}
