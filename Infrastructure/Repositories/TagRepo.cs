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




        public async Task<Tag?> HasValue(string tagSlug, string tag)
        {
            return await _context.Tags.FirstOrDefaultAsync(e => e.Name == tag && e.Slug == tagSlug);
        }

        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
