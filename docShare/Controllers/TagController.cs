using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/tags")]
    [ApiController]
    [EnableRateLimiting("read_public")]
    public class TagController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        private readonly IMemoryCache _cache;

        public TagController(IUnitOfWork repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetTag([FromQuery] int take = 10)
        {
            string cacheKey = $"tags";

            if (!_cache.TryGetValue(cacheKey, out List<TagsDto>? tags))
            {
                tags = await _repo.tagsRepo.GetTags(take);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                _cache.Set(cacheKey, tags, cacheOptions);
            }
            
            return Ok(tags);
        }

        [HttpGet("documents")]
        public async Task<IActionResult> GetDocumentsByTag([FromQuery] int? tagid, [FromQuery] int skip, [FromQuery] int take)
        {
            string cacheKey = $"tag_docs_{tagid}_{skip}_{take}";
            if (!_cache.TryGetValue(cacheKey, out List<ResSummaryDocumentDto>? result))
            {
                result = await _repo.tagsRepo.GetDocumentByTagID(tagid, skip, take);
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                _cache.Set(cacheKey, result, cacheOptions);
            }

            return Ok(result);
        }
    }
}
