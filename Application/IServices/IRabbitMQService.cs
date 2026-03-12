using System.Threading.Tasks;
using Application.DTOs;

namespace Application.IServices
{
    public interface IRabbitMQService
    {
        Task SendFileToScan(string filePath, string userId, string documentidto);
        Task SendThumbnailRequest(ThumbRequestEvent message);
        Task SendEmailResquest(SendMailRequestDto request);
    }
}
