using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUniversitites
    {
        public Task<bool> HasValue(int uniId);
        public Task<UniversitySection> AddSectionToUniversityAsync(int uniId, string name);
        public Task<bool> HasUniSection(int sectionId);
        public Task SaveChangeAsync();
    }
}
