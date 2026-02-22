using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUniversitites
    {
        public Task<bool> HasValue(int uniId);
        public Task<List<University>> GetUniversityAsync();
        public Task<List<UniversitySection>?> GetUniversitySectionsAsync(int uniId);
        public Task<UniversitySection> AddSectionToUniversityAsync(int uniId, string name);
        public Task<bool> HasUniSection(int uniId, int sectionId);
    }
}
