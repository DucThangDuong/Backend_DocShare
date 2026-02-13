using Application.DTOs;
using Domain.Entities;
namespace Application.Interfaces
{
    public interface ITags
    {
        public Task<Tag?> HasTag(string tagSlug, string tag);
        public Task CreateTag(Tag tag);
        public Task<List<string>?> GetTagOfDocument(int docid);
        public Task RemoveAllTagsByDocIdAsync(int docId);
        public Task<List<Tag>> GetTags(int take);
        public Task<List<ResDocumentDto>> GetDocumentByTagID(int? tagid,int skip,int take);
    }
}
