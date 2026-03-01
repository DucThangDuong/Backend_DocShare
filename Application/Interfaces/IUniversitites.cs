using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUniversitites
    {
        public Task<bool> HasValue(int uniId);
        public Task<List<University>> GetUniversityAsync();
        public Task<List<UniversitySection>?> GetUniversitySectionsAsync(int uniId);
        public Task<UniversitySection> AddSectionToUniversityAsync(int uniId, string name);
        public Task<bool> HasUniSection(int sectionId);
        public Task<List<ResSummaryDocumentDto>> GetDocOfSection(int sectionId);
        public Task<List<ResSummaryDocumentDto>> GetDocOfUniversity(int universityId, int skip, int take);
        public Task<List<ResSummaryDocumentDto>> GetPopularDocuments(int universityId, int skip, int take);
        public Task<List<ResSummaryDocumentDto>> GetRecentDocuments(int universityId, int skip, int take);
        public Task SaveChangeAsync();
    }
}
