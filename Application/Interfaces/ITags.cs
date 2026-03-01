using Application.DTOs;
using Domain.Entities;
namespace Application.Interfaces
{
    public interface ITags
    {
        public Task<Tag?> HasValue(string tagSlug, string tag);
        public void Create(Tag tag);
        public Task<List<string>?> GetTagOfDocument(int docid);
        public Task RemoveAllTagsOfDocIdAsync(int docId);
        public Task<List<TagsDto>> GetTags(int take);
        public Task<List<ResSummaryDocumentDto>> GetDocumentByTagID(int? tagid,int skip,int take);
        public Task SaveChangeAsync();
    }
}
