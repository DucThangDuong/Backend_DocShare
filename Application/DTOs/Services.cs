using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AuthResultDTO
    {
        public bool IsSuccess { get; set; }
        public string? CustomJwtToken { get; set; }
        public string? ErrorMessage { get; set; }
        public RefreshToken refreshToken { get; set; } = null!;
    }
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
    }
    public class ScanFileResultDto
    {
        public string? FilePath { get; set; }
        public string? Status { get; set; }
        public string? UserId { get; set; }
        public string? DocIdDto { get; set; }
        public string? Message { get; set; }
    }
    public class ThumbRequestEvent
    {
        public long DocId { get; set; }
        public string FileUrl { get; set; }      
        public string BucketName { get; set; }   
    }

    public class ThumbResponseEvent
    {
        public int DocId { get; set; }
        public string ThumbnailUrl { get; set; } 
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
    public enum StorageType
    {
        Document,
        Avatar
    }
}
