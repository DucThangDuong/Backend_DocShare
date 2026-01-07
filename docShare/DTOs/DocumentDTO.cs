namespace API.DTOs
{
    public class ReqCreateDocumentDTO
    {
        public IFormFile File { get; set; } 
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public string? Status  { get; set; } 
        public string? SignalRConnectionID { get; set; }
    }
    public class DocumentResponseDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public long SizeInBytes { get; set; }
        public string FileUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class UserStorageFileDto
    {
        public long StorageLimit { get; set; }
        public long UsedStorage { get; set; }
        public int TotalCount { get; set; }
        public int Trash {  get; set; }
    }
}
