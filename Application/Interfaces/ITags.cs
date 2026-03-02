using Application.DTOs;
using Domain.Entities;
namespace Application.Interfaces
{
    public interface ITags
    {
        public Task<Tag?> HasValue(string tagSlug, string tag);
        public void Create(Tag tag);


        public Task RemoveAllTagsOfDocIdAsync(int docId);
        public Task SaveChangeAsync();
    }
}
