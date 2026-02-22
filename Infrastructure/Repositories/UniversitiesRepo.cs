using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<List<University>> GetUniversityAsync()
        {
            return await _context.Universities.AsNoTracking().ToListAsync();
        }

        public async Task<List<UniversitySection>?> GetUniversitySectionsAsync(int uniId)
        {
            bool ishas = await _context.Universities.AsNoTracking().AnyAsync(e => e.Id == uniId);
            if (!ishas) return null;
            return await _context.UniversitySections.AsNoTracking().Where(e => e.UniversityId == uniId).ToListAsync();
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
        public async Task<bool> HasUniSection(int uniId, int sectionId)
        {
            return await _context.UniversitySections.AsNoTracking().AnyAsync(e => e.Id == sectionId && e.UniversityId == uniId);
        }
    }
}
