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

        public void  Create(Document document)
        {
             _context.Documents.Add(document);
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

        public async Task<List<ResDocumentDetailEditDto>> GetDocsByUserIdPagedAsync(int userId, int skip, int take)
        {
            return await _context.Documents
                .AsNoTracking()
                .Where(d => d.UploaderId == userId && d.IsDeleted == 0)
                .OrderByDescending(d => d.CreatedAt)
                .Skip(skip).Take(take).Select(d => new ResDocumentDetailEditDto
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    Description = d.Description,
                    FileUrl = d.FileUrl,
                    Title = d.Title,
                    Thumbnail = d.Thumbnail,
                    PageCount = d.PageCount,
                    SizeInBytes = d.SizeInBytes,
                    UpdatedAt = d.UpdatedAt,
                    Status = d.Status,
                    Tags = d.Tags.Select(e => e.Name).ToList(),
                }).ToListAsync();
        }

        public async Task<ResDocumentDetailDto?> GetDocByUserIDAsync(int docID, int? currentUserId)
        {
            var query = _context.Documents
                .AsNoTracking()
                .Where(e => e.Id == docID)
                .Select(d => new ResDocumentDetailDto
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    UploaderId= d.UploaderId,
                    Description = d.Description,
                    FileUrl = d.FileUrl,
                    Title = d.Title,
                    AvatarUrl = d.Uploader.LoginProvider == "Custom" ? d.Uploader.CustomAvatar : d.Uploader.GoogleAvatar,
                    FullName = d.Uploader.FullName,
                    LikeCount = d.LikeCount,
                    ViewCount = d.ViewCount,
                    Thumbnail = d.Thumbnail,
                    PageCount = d.PageCount,
                    IsLiked = _context.DocumentVotes
                                .Where(v => v.DocumentId == d.Id && v.UserId == currentUserId)
                                .Select(v => (bool?)v.IsLike).FirstOrDefault(),
                    IsSaved = _context.SavedDocuments.Any(s => s.DocumentId == d.Id && s.UserId == currentUserId),
                    Tags = d.Tags.Select(e => e.Name).ToList()
                });
            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> HasValue(int docID)
        {
            return await _context.Documents.AsNoTracking().AnyAsync(e => e.Id == docID);
        }

        public void Update(Document document)
        {
            _context.Documents.Update(document);

        }

        public async Task ClearFileContentUrl(int docid)
        {
            var doc = await _context.Documents.FirstOrDefaultAsync(e => e.Id == docid);
            doc!.FileUrl = String.Empty;
            doc.SizeInBytes = 0;
            doc.Thumbnail = null;
            doc.UpdatedAt = DateTime.UtcNow;
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

        public async Task<ResDocumentDetailEditDto?> GetDocumentDetailEditAsync(int userId, int docId)
        {
            var query = _context.Documents
                .AsNoTracking()
                .Where(e => e.Id == docId && e.UploaderId == userId)
                .Select(d => new ResDocumentDetailEditDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    CreatedAt = d.CreatedAt,
                    Thumbnail = d.Thumbnail,
                    PageCount = d.PageCount,
                    Tags = d.Tags.Select(e => e.Name).ToList(),
                    Description = d.Description,
                    SizeInBytes = d.SizeInBytes,
                    Status = d.Status,
                    FileUrl = d.FileUrl,
                    UpdatedAt=d.UpdatedAt,
                    UniversitySectionId=d.UniversitySectionId,
                    UniversityId=d.UniversitySection.UniversityId
                });
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<ResSummaryDocumentDto>>? GetDocumentSaveOfUser(int userId)
        {
            return await _context.SavedDocuments
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .Select(s => new ResSummaryDocumentDto
                {
                    Id = s.Document.Id,
                    Title = s.Document.Title,
                    CreatedAt = s.Document.CreatedAt,
                    Thumbnail = s.Document.Thumbnail,
                    PageCount = s.Document.PageCount,
                    LikeCount = s.Document.LikeCount
                }).ToListAsync();
        }

        public async Task<List<ResSummaryDocumentDto>>? GetDocumentLikeOfUser(int userId)
        {
            return await _context.DocumentVotes
                .AsNoTracking()
                .Where(v => v.UserId == userId && v.IsLike)
                .Select(v => new ResSummaryDocumentDto
                {
                    Id = v.Document.Id,
                    Title = v.Document.Title,
                    CreatedAt = v.Document.CreatedAt,
                    Thumbnail = v.Document.Thumbnail,
                    PageCount = v.Document.PageCount,
                    LikeCount = v.Document.LikeCount
                }).ToListAsync();
        }

        public async Task<List<ResSummaryDocumentDto>>? GetDocumentUploadOfUser(int userId)
        {
            return await _context.Documents
                .AsNoTracking()
                .Where(d => d.UploaderId == userId && d.IsDeleted == 0)
                .Select(d => new ResSummaryDocumentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    CreatedAt = d.CreatedAt,
                    Thumbnail = d.Thumbnail,
                    PageCount = d.PageCount,
                    LikeCount = d.LikeCount
                }).ToListAsync();
        }
        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
