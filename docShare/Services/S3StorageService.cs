using Amazon.S3;
using Amazon.S3.Model;
using Application.IServices;

namespace API.Services
{
    public class S3StorageService:IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3StorageService(IAmazonS3 s3Client, IConfiguration config)
        {
            _s3Client = s3Client;
            _bucketName = config["Storage:BucketName"] ?? "";
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);
            return $"{_s3Client.Config.ServiceURL}/{_bucketName}/{fileName}";
        }
        public async Task DeleteFileAsync(string fileName)
        {
            await _s3Client.DeleteObjectAsync(_bucketName, fileName);
        }
        public async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(_bucketName, fileName);
                return true; 
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false; 
                }
                throw; 
            }
        }
        public async Task<Stream> GetFileStreamAsync(string fileName)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                var response = await _s3Client.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"Không thể tải file từ S3: {fileName}. Lỗi: {ex.Message}");
            }
        }
    }
}
