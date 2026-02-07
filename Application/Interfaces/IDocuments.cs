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
        public Task<bool> CreateAsync(Document document);
        public Task<int> CountDocByUserID(int UserID);
        public Task<int> CountTrashByUserID(int UserID);
        public Task<List<ResDocumentDto>> GetDocsByUserIdPagedAsync(int userId, int skip, int take);
        public Task<Document?> GetDocByIDAsync(int docId);
        public Task<ResDocumentDto?> GetDocWithUserByUserID(int docID,int currentUserId);
        public Task<bool> HasDocument(int docID);
        public Task<bool> MoveToTrash(int docID);
        public Task<bool> UpdateAsync(Document document);
        public Task<bool> DeleteFileUrl(int docid);
    }
}
