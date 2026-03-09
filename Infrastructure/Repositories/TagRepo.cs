using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;


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

        public async Task ClearTagsByDocIdAsync(int docId)
        {
            var document = await _context.Documents
                                         .Include(d => d.Tags)
                                         .FirstOrDefaultAsync(d => d.Id == docId);

            if (document != null)
            {
                document.Tags.Clear();
            }
        }




        public async Task<Tag?> GetBySlugAndNameAsync(string tagSlug, string name)
        {
            return await _context.Tags.FirstOrDefaultAsync(e => e.Name == name && e.Slug == tagSlug);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
