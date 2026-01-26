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
            return await _context.Documents.CountAsync(e => e.UploaderId == UserID && e.IsDeleted == 0);
        }

        public async Task<int> CountTrashByUserID(int UserID)
        {
            return await _context.Documents.CountAsync(e => e.UploaderId == UserID && e.IsDeleted == 1);
        }

        public async Task<bool> CreateAsync(Document document)
        {
            try
            {
                await _context.Documents.AddAsync(document);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> MoveToTrash(int docID)
        {
            var docchange = await _context.Documents.FirstOrDefaultAsync(e => e.Id == docID);
            if (docchange == null) return false;
            docchange.IsDeleted = 1;
            _context.Documents.Update(docchange);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<Document?> GetDocByIDAsync(int docId)
        {
            return await _context.Documents.FirstOrDefaultAsync(e=>e.Id == docId);
        }

        public async Task<List<ResDocumentDto>> GetDocsByUserIdPagedAsync(int userId, int skip, int take)
        {
            return await _context.Documents
                .Where(d => d.UploaderId == userId && d.IsDeleted == 0)
                .OrderByDescending(d => d.CreatedAt)
                .Skip(skip).Take(take).Select(d =>new ResDocumentDto
                {
                    Id = d.Id,
                    SizeInBytes = d.SizeInBytes,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    Description = d.Description,
                    FileUrl = d.FileUrl,
                    Title = d.Title,
                }).ToListAsync();
        }

        public async Task<ResDocumentDto?> GetDocWithUserByUserID(int docID)
        {
            return await _context.Documents.Where(e => e.Id==docID).Select(d=>new ResDocumentDto
            {
                Id = d.Id,
                CreatedAt= d.CreatedAt,
                Description = d.Description,
                FileUrl = d.FileUrl,
                Title = d.Title,
                SizeInBytes= d.SizeInBytes,
                Status = d.Status,
                AvatarUrl= d.Uploader.AvatarUrl,
                FullName = d.Uploader.FullName,
                DislikeCount=d.DislikeCount,
                LikeCount=d.LikeCount,
                ViewCount=d.ViewCount,
            }).FirstOrDefaultAsync();
        }

        public async Task<bool> HasDocument(int docID)
        {
            return await _context.Documents.AnyAsync(e=>e.Id==docID);
        }
    }
}
