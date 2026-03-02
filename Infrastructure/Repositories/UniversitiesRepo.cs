using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Repositories
{
    public class UniversitiesRepo : IUniversitites
    {
        public readonly DocShareContext _context;
        public UniversitiesRepo(DocShareContext context)
        {
            _context = context;
        }
        public async Task<bool> HasValue(int uniId)
        {
            return await _context.Universities.AsNoTracking().AnyAsync(e => e.Id == uniId);
        }



        public async Task<UniversitySection> AddSectionToUniversityAsync(int uniId, string name)
        {
            UniversitySection section = new UniversitySection
            {
                Name = name,
                UniversityId = uniId
            };
            await _context.UniversitySections.AddAsync(section);
            return section;
        }
        public async Task<bool> HasUniSection(int sectionId)
        {
            return await _context.UniversitySections.AsNoTracking().AnyAsync(e => e.Id == sectionId);
        }


        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
