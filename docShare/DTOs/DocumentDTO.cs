using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ReqCreateDocumentDTO
    {
        public IFormFile? File { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string>? Tags { get; set; }
        public string? Status { get; set; }
        public string? userId { get; set; }
        public int? UniversityId { get; set; }
        public int? UniversitySectionId { get; set; }
    }
    public class ReqMoveToTrashDTO
    {
        public bool isDeleted { get; set; }
    }
    public class ReqUpdateDocumentDto
    {
        public IFormFile? File { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string>? Tags { get; set; }
        public string? Status { get; set; }
        public int? UniversityId { get; set; }
        public int? UniversitySectionId { get; set; }
    }
}
