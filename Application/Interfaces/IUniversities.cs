using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUniversities
    {
        public Task<bool> ExistsAsync(int uniId);
        public Task<UniversitySection> AddSectionToUniversityAsync(int uniId, string name);
        public Task<bool> SectionExistsAsync(int sectionId);
        public Task SaveChangesAsync();
    }
}
