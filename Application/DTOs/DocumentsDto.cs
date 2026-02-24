namespace Application.DTOs
{
    public class ResDocumentDetailEditDto : ResDocumentBaseDto
    {
        public string? Description { get; set; }
        public long SizeInBytes { get; set; }
        public string? Status { get; set; }
        public string FileUrl { get; set; } = null!;
        public DateTime? UpdatedAt { get; set; }
        public int? UniversitySectionId { get; set; }
        public int? UniversityId { get; set; }
    }
    public class ResDocumentDetailDto : ResDocumentBaseDto
    {
        public string? Description { get; set; }
        public int UploaderId { get; set; }
        public string FileUrl { get; set; } = null!;
        public int? LikeCount { get; set; }
        public bool? IsLiked { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public int? ViewCount { get; set; }
        public bool? IsSaved { get; set; }

    }
    public class ResSummaryDocumentDto:ResDocumentBaseDto
    {
        public int? LikeCount { get; set; }
    }
    public class ResDocumentBaseDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public string? Thumbnail { get; set; }
        public int? PageCount { get; set; }
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
