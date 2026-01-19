using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task DeleteFileAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
        Task<Stream> GetFileStreamAsync(string fileName);
    }
}
