using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class DocumentsRepo : IDocuments
    {
        private readonly DocShareContext _context;
        public DocumentsRepo(DocShareContext context)
        {
            _context = context;
        }

        public async Task<int> CountDocByUserID(int UserID)
        {
            return await _context.Documents.AsNoTracking().CountAsync(e => e.UploaderId == UserID && e.IsDeleted == 0);
        }

        public async Task<int> CountTrashByUserID(int UserID)
        {
            return await _context.Documents.AsNoTracking().CountAsync(e => e.UploaderId == UserID && e.IsDeleted == 1);
        }

        public async Task CreateAsync(Document document)
        {
            await _context.Documents.AddAsync(document);
        }

        public async Task MoveToTrash(int docID)
        {
            var docchange = await _context.Documents.FirstOrDefaultAsync(e => e.Id == docID);
            docchange!.IsDeleted = 1;
            _context.Documents.Update(docchange);
        }

        public async Task<Document?> GetDocByIDAsync(int docId)
        {
            return await _context.Documents.FirstOrDefaultAsync(e => e.Id == docId);
        }

        public async Task<List<ResDocumentDto>> GetDocsByUserIdPagedAsync(int userId, int skip, int take)
        {
            return await _context.Documents
                .AsNoTracking()
                .Where(d => d.UploaderId == userId && d.IsDeleted == 0)
                .OrderByDescending(d => d.CreatedAt)
                .Skip(skip).Take(take).Select(d => new ResDocumentDto
                {
                    Id = d.Id,
                    SizeInBytes = d.SizeInBytes,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    Description = d.Description,
                    FileUrl = d.FileUrl,
                    Title = d.Title,
                    Thumbnail=d.Thumbnail,
                    PageCount = d.PageCount,
                }).ToListAsync();
        }

        public async Task<ResDocumentDto?> GetDocByUserIDAsync(int docID, int currentUserId)
        {
            var query = _context.Documents
                .AsNoTracking()
                .Where(e => e.Id == docID)
                .Select(d => new ResDocumentDto
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    Description = d.Description,
                    FileUrl = d.FileUrl,
                    Title = d.Title,
                    SizeInBytes = d.SizeInBytes,
                    Status = d.Status,
                    CustomAvatar = d.Uploader.CustomAvatar,
                    GoogleAvatar = d.Uploader.GoogleAvatar,
                    FullName = d.Uploader.FullName,
                    DislikeCount = d.DislikeCount,
                    LikeCount = d.LikeCount,
                    ViewCount = d.ViewCount,
                    Thumbnail=d.Thumbnail,
                    PageCount=d.PageCount,
                    IsLiked = _context.DocumentVotes
                                .Where(v => v.DocumentId == d.Id && v.UserId == currentUserId)
                                .Select(v => (bool?)v.IsLike).FirstOrDefault(),
                    IsSaved = _context.SavedDocuments.Any(s => s.DocumentId == d.Id && s.UserId == currentUserId),
                    Tags = d.Tags.Select(e => e.Name).ToList()
                });
            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> HasDocument(int docID)
        {
            return await _context.Documents.AsNoTracking().AnyAsync(e => e.Id == docID);
        }

        public async Task UpdateAsync(Document document)
        {
            _context.Documents.Update(document);

        }

        public async Task DeleteFileUrl(int docid)
        {
            var doc = await _context.Documents.FirstOrDefaultAsync(e => e.Id == docid);
            doc!.FileUrl = String.Empty;
            doc.SizeInBytes = 0;
            doc.Thumbnail = null;
            doc.CreatedAt = null;
            doc.PageCount = 0;
        }

        public async Task<ResUserStatsDto?> GetUserStatsAsync(int userId)
        {
            var stats = await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new ResUserStatsDto
            {
                UploadCount = _context.Documents.Count(d => d.UploaderId == userId && d.IsDeleted == 0),
                SavedCount = _context.SavedDocuments.Count(s => s.UserId == userId),
                TotalLikesReceived = _context.Documents.Where(e => e.UploaderId == userId).Select(e => (int?)e.LikeCount).Sum() ?? 0
            })
            .FirstOrDefaultAsync();

            return stats;
        }
    }
}
