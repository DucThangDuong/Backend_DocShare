using API.DTOs;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace API.Controllers
{
    [Route("api/universities")]
    [ApiController]
    public class UniversitiesController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        private readonly IMemoryCache _cache;
        public UniversitiesController(IUnitOfWork repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            string cacheKey = "universities";
            if (_cache.TryGetValue(cacheKey, out List<University>? result))
            {
                return Ok(result);
            }
            result = await _repo.universititesRepo.GetUniversityAsync();
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));
            _cache.Set(cacheKey, result, cacheOptions);
            return Ok(result);
        }

        [HttpGet("{universityId}/sections")]
        public async Task<IActionResult> GetSectionOfUniversity(int universityId)
        {
            bool ishas = await _repo.universititesRepo.HasValue(universityId);
            if (!ishas) return NotFound();
            List<UniversitySection>? result = await _repo.universititesRepo.GetUniversitySectionsAsync(universityId);
            return Ok(result);
        }

        [HttpGet("sections/{sectionId}/documents")]
        public async Task<IActionResult> GetDocumentsOfSection(int sectionId)
        {
            bool ishas = await _repo.universititesRepo.HasUniSection(sectionId);
            if (!ishas) return NotFound();
            List<ResSummaryDocumentDto> result = await _repo.universititesRepo.GetDocOfSection(sectionId);
            return Ok(result);
        }

        // GET api/universities/{universityId}/documents

        [HttpGet("{universityId}/documents/popular")]
        public async Task<IActionResult> GetPopularDocuments(int universityId, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            bool ishas = await _repo.universititesRepo.HasValue(universityId);
            if (!ishas) return NotFound();

            string cacheKey = $"universities:{universityId}:documents:popular:{skip}:{take}";
            if (_cache.TryGetValue(cacheKey, out List<ResSummaryDocumentDto>? result))
            {
                return Ok(result);
            }

            result = await _repo.universititesRepo.GetPopularDocuments(universityId, skip, take);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));
            _cache.Set(cacheKey, result, cacheOptions);
            return Ok(result);
        }

        [HttpGet("{universityId}/documents/recent")]
        public async Task<IActionResult> GetRecentDocuments(int universityId, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            bool ishas = await _repo.universititesRepo.HasValue(universityId);
            if (!ishas) return NotFound();

            string cacheKey = $"universities:{universityId}:documents:recent:{skip}:{take}";
            if (_cache.TryGetValue(cacheKey, out List<ResSummaryDocumentDto>? result))
            {
                return Ok(result);
            }

            result = await _repo.universititesRepo.GetRecentDocuments(universityId, skip, take);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            _cache.Set(cacheKey, result, cacheOptions);
            return Ok(result);
        }

        [HttpPost("{universityId}/sections")]
        public async Task<IActionResult> AddSectionToUniversity(int universityId, [FromBody] ReqUniversitySectionDTO section)
        {
            bool ishas = await _repo.universititesRepo.HasValue(universityId);
            if (!ishas) return NotFound();
            UniversitySection result = await _repo.universititesRepo.AddSectionToUniversityAsync(universityId, section.Name);
            await _repo.SaveAllAsync();
            return Ok(result);
        }
    }
}

