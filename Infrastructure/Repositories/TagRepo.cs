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
        public DocShareContext _context;
        public TagRepo(DocShareContext context)
        {
            _context = context;
        }

        public async Task Create(Tag tag)
        {
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAllTagsByDocIdAsync(int docId)
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

        public async Task<Tag?> HasTag(string tagSlug, string tag)
        {
            return await _context.Tags.FirstOrDefaultAsync(e=> e.Name==tag && e.Slug==tagSlug);
        }


    }
}
