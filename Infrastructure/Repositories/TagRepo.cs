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

        public async Task<Tag?> HasTag(string Slug, string tag)
        {
            return await _context.Tags.FirstOrDefaultAsync(e=>e.Slug==Slug && e.Name==tag);
        }
    }
}
