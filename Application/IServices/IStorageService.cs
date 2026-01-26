using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
namespace Application.IServices
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, StorageType type);
        Task DeleteFileAsync(string fileName, StorageType type);
        Task<bool> FileExistsAsync(string fileName, StorageType type);
        Task<Stream> GetFileStreamAsync(string fileName, StorageType type);
    }
}
