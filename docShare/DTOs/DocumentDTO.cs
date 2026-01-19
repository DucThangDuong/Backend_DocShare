namespace API.DTOs
{
    public class ReqCreateDocumentDTO
    {
        public IFormFile File { get; set; } 
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public string? Status  { get; set; } 
        public string? userId { get; set; }
    }
}
