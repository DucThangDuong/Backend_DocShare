using Application.DTOs;
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
        public async Task<bool> HasUniSection(int sectionId)
        {
            return await _context.UniversitySections.AsNoTracking().AnyAsync(e => e.Id == sectionId);
        }

        public async Task<List<ResSummaryDocumentDto>> GetDocOfSection(int sectionId)
        {
            return await _context.UniversitySections.AsNoTracking()
                .Where(e => e.Id == sectionId)
                .SelectMany(e => e.Documents)
                .Select(d => new ResSummaryDocumentDto
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    Title = d.Title,
                    LikeCount = d.LikeCount,
                    Thumbnail = d.Thumbnail,
                    PageCount = d.PageCount,
                    Tags = d.Tags.Select(t => t.Name).ToList(),

                }).ToListAsync();
        }

        public Task<List<ResSummaryDocumentDto>> GetDocOfUniversity(int universityId,int skip,int take)
        {
            return _context.Universities.AsNoTracking()
                .Where(e => e.Id == universityId)
                .SelectMany(e => e.UniversitySections)
                .SelectMany(s => s.Documents)
                .Skip(skip).Take(take)
                .Select(d => new ResSummaryDocumentDto
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    Title = d.Title,
                    LikeCount = d.LikeCount,
                    Thumbnail = d.Thumbnail,
                    PageCount = d.PageCount,
                    Tags = d.Tags.Select(t => t.Name).ToList(),
                }).ToListAsync();
        }

        public Task<List<ResSummaryDocumentDto>> GetPopularDocuments(int universityId, int skip, int take)
        {
            return _context.Universities.AsNoTracking()
                .Where(e => e.Id == universityId)
                .SelectMany(e => e.UniversitySections)
                .SelectMany(s => s.Documents)
                .OrderByDescending(d => d.LikeCount)
                .Skip(skip).Take(take)
                .Select(d => new ResSummaryDocumentDto
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    Title = d.Title,
                    LikeCount = d.LikeCount,
                    Thumbnail = d.Thumbnail,
                    PageCount = d.PageCount,
                    Tags = d.Tags.Select(t => t.Name).ToList(),
                }).ToListAsync();
        }

        public Task<List<ResSummaryDocumentDto>> GetRecentDocuments(int universityId, int skip, int take)
        {
            return _context.Universities.AsNoTracking()
                .Where(e => e.Id == universityId)
                .SelectMany(e => e.UniversitySections)
                .SelectMany(s => s.Documents)
                .OrderByDescending(d => d.CreatedAt)
                .Skip(skip).Take(take)
                .Select(d => new ResSummaryDocumentDto
                {
                    Id = d.Id,
                    CreatedAt = d.CreatedAt,
                    Title = d.Title,
                    LikeCount = d.LikeCount,
                    Thumbnail = d.Thumbnail,
                    PageCount = d.PageCount,
                    Tags = d.Tags.Select(t => t.Name).ToList(),
                }).ToListAsync();
        }
    }
}
