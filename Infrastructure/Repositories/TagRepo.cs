using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TagRepo : ITags
    {
        private readonly DocShareContext _context;
        public TagRepo(DocShareContext context)
        {
            _context = context;
        }

        public void Create(Tag tag)
        {
            _context.Tags.Add(tag);
        }

        public async Task RemoveAllTagsOfDocIdAsync(int docId)
        {
            var document = await _context.Documents
                                         .Include(d => d.Tags)
                                         .FirstOrDefaultAsync(d => d.Id == docId);

            if (document != null)
            {
                document.Tags.Clear();
            }
        }

        public async Task<List<string>?> GetTagOfDocument(int docid)
        {
            return await _context.Documents
                    .Where(d => d.Id == docid)
                    .SelectMany(d => d.Tags.Select(t => t.Name))
                    .ToListAsync();
        }

        public async Task<Tag?> HasValue(string tagSlug, string tag)
        {
            return await _context.Tags.FirstOrDefaultAsync(e => e.Name == tag && e.Slug == tagSlug);
        }

        public async Task<List<TagsDto>> GetTags(int take)
        {
            return await _context.Tags
                .AsNoTracking()
                .OrderByDescending(t => t.Documents.Count)
                .Take(take)
                .Select(t => new TagsDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    count = t.Documents.Count
                })
                .ToListAsync();
        }

        public async Task<List<ResSummaryDocumentDto>> GetDocumentByTagID(int? tagid, int skip, int take)
        {
            var query = _context.Documents.AsNoTracking();
            query = query.Where(e => e.FileUrl != null && e.FileUrl != "");

            if (tagid.HasValue && tagid.Value > 0)
            {
                int id = tagid.Value;
                query = query.Where(d => d.Tags.Any(t => t.Id == id));
            }

            query = query.OrderByDescending(d => d.CreatedAt).Skip(skip).Take(take);

            return await query
                 .Select(d => new ResSummaryDocumentDto
                 {
                     Id = d.Id,
                     CreatedAt = d.CreatedAt,
                     Title = d.Title,
                     LikeCount = d.LikeCount,
                     Thumbnail = d.Thumbnail,
                     PageCount = d.PageCount,
                     Tags = d.Tags.Select(t => t.Name).ToList(),

                 }).ToListAsync();
        }
    }
}
