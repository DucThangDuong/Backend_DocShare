using Application.DTOs;
using Domain.Entities;
namespace Application.Interfaces
{
    public interface ITags
    {
        public Task<Tag?> GetBySlugAndNameAsync(string tagSlug, string name);
        public void Create(Tag tag);


        public Task ClearTagsByDocIdAsync(int docId);
        public Task SaveChangesAsync();
    }
}
