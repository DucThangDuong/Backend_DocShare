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
        public Task CreateAsync(Document document);
        public Task<int> CountDocByUserID(int UserID);
        public Task<int> CountTrashByUserID(int UserID);
        public Task<List<ResDocumentDto>> GetDocsByUserIdPagedAsync(int userId, int skip, int take);
        public Task<Document?> GetDocByIDAsync(int docId);
        public Task<ResDocumentDto?> GetDocByUserIDAsync(int docID,int currentUserId);
        public Task<bool> HasDocument(int docID);
        public Task MoveToTrash(int docID);
        public Task UpdateAsync(Document document);
        public Task DeleteFileUrl(int docid);
        public Task<ResUserStatsDto?> GetUserStatsAsync(int userId);
    }
}
