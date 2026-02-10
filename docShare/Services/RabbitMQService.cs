using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace API.Services
{
    public class RabbitMQService : IDisposable
    {
        private readonly ILogger<RabbitMQService> _logger;
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMQService(IConfiguration config, ILogger<RabbitMQService> logger)
        {
            _logger = logger;
            var hostName = config["RabbitMQ:HostName"] ?? "localhost";

            var factory = new ConnectionFactory { HostName = hostName };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel =  _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _channel.QueueDeclareAsync(queue: "pdf_scan_queue", durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
            _channel.QueueDeclareAsync(queue: "thumb_create_queue", durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
        }
        public async Task SendFileToScan(string filePath, string userId, string documentidto)
        {
            try
            {
                var message = new { FilePath = filePath, UserId = userId, DocIdDto = documentidto };
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                await _channel.BasicPublishAsync(exchange: "", routingKey: "pdf_scan_queue", body: body);

                _logger.LogInformation("Sent file to scan queue: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send file to scan queue");
                throw;
            }
        }

        public async Task SendThumbnailRequest(ThumbRequestEvent message)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                await _channel.BasicPublishAsync(exchange: "", routingKey: "thumb_create_queue", body: body);

                _logger.LogInformation("Sent thumb request for DocId: {DocId}", message.DocId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send thumb request");
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

    public class RabbitMQWorker : BackgroundService
    {
        private readonly ISignalRService _signalRService;
        private readonly IConfiguration _config;
        private readonly IStorageService _storageService;
        private readonly IServiceScopeFactory _scopeFactory; 
        private readonly ILogger<RabbitMQWorker> _logger;
        private readonly string _hostName;

        public RabbitMQWorker(
            ISignalRService signalRService,
            IConfiguration configuration,
            IStorageService storageService,
            IServiceScopeFactory scopeFactory, 
            ILogger<RabbitMQWorker> logger)
        {
            _signalRService = signalRService;
            _config = configuration;
            _storageService = storageService;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hostName = _config["RabbitMQ:HostName"] ?? "localhost";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var factory = new ConnectionFactory { HostName = _hostName };
                    using var connection = await factory.CreateConnectionAsync();
                    using var channel = await connection.CreateChannelAsync();

                    // create check virus document
                    await channel.QueueDeclareAsync(queue: "pdf_result_queue", durable: true, false, false);
                    var consumerPdf = new AsyncEventingBasicConsumer(channel);

                    consumerPdf.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var result = JsonSerializer.Deserialize<ScanFileResultDto>(message);

                        if (result != null && !string.IsNullOrEmpty(result.UserId))
                        {
                            await _signalRService.NotifyScanResultAsync(result.UserId, result);
                            if (result.FilePath != null)
                            {
                                await DeleteFileAsync(result.FilePath);
                            }
                        }
                    };
                    await channel.BasicConsumeAsync(queue: "pdf_result_queue", autoAck: true, consumer: consumerPdf);

                    // create thumbnail
                    await channel.QueueDeclareAsync(queue: "thumb_result_queue", durable: true, false, false);
                    var consumerThumb = new AsyncEventingBasicConsumer(channel);

                    consumerThumb.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        try
                        {
                            var result = JsonSerializer.Deserialize<ThumbResponseEvent>(message);
                            if (result != null && result.IsSuccess)
                            {
                                await UpdateThumbnailInDb(result);
                            }
                            else if (result != null && !result.IsSuccess)
                            {
                                _logger.LogError("Thumb worker failed: {Error}", result.ErrorMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing thumb result");
                        }
                    };
                    await channel.BasicConsumeAsync(queue: "thumb_result_queue", autoAck: true, consumer: consumerThumb);

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RabbitMQWorker, retrying in 5 seconds...");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private async Task UpdateThumbnailInDb(ThumbResponseEvent result)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var document = await unitOfWork.documentsRepo.GetDocByIDAsync(result.DocId);
                if (document != null)
                {
                    document.Thumbnail = result.ThumbnailUrl;
                    await unitOfWork.documentsRepo.UpdateAsync(document);
                    await unitOfWork.SaveAllAsync();

                    _logger.LogInformation("Updated Thumbnail for DocId: {Id}", result.DocId);
                }
            }
        }

        private async Task<bool> DeleteFileAsync(string storagePath)
        {
            try
            {
                if (await _storageService.FileExistsAsync(storagePath, StorageType.Document))
                {
                    await _storageService.DeleteFileAsync(storagePath, StorageType.Document);
                    _logger.LogInformation("Deleted file: {StoragePath}", storagePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {StoragePath}", storagePath);
                return false;
            }
        }
    }
}