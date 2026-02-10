using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ResDocumentDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string FileUrl { get; set; } = null!;
        public long SizeInBytes { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public int? ViewCount { get; set; }
        public int? LikeCount { get; set; }
        public int? DislikeCount { get; set; }
        public bool? IsLiked { get; set; } 
        public bool? IsSaved { get; set; }
        public string? Thumbnail { get; set; }
        public List<string>? Tags { get; set; }
    }
    public class ResUserStorageFileDto
    {
        public long StorageLimit { get; set; }
        public long UsedStorage { get; set; }
        public int TotalCount { get; set; }
        public int Trash { get; set; }
    }
    public class ResUserStatsDto
    {
        public int UploadCount { get; set; }
        public int SavedCount {  get; set; }
        public int TotalLikesReceived {  get; set; }
    }
}
