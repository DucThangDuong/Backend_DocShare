using Application.DTOs;
using Application.IServices;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace API.Services
{
    public class RabbitMQService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly string _hostName;

        public RabbitMQService(IConfiguration config, ILogger<RabbitMQService> logger)
        {
            _config = config;
            _logger = logger;
            _hostName = _config["RabbitMQ:HostName"] ?? "localhost";
        }

        public async Task SendFileToScan(string filePath, string userId, string documentidto)
        {
            try
            {
                var factory = new ConnectionFactory { HostName = _hostName };
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();
                await channel.QueueDeclareAsync(queue: "pdf_scan_queue", durable: true, exclusive: false, autoDelete: false);

                var message = new { FilePath = filePath, UserId = userId, DocIdDto = documentidto };
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                await channel.BasicPublishAsync(exchange: "", routingKey: "pdf_scan_queue", body: body);

                _logger.LogInformation("Sent file to scan queue: {FilePath} for user {UserId}", filePath, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send file to scan queue: {FilePath}", filePath);
                throw;
            }
        }
    }

    public class RabbitMQWorker : BackgroundService
    {
        private readonly ISignalRService _signalRService;
        private readonly IConfiguration _config;
        private readonly IStorageService _storageService;
        private readonly ILogger<RabbitMQWorker> _logger;
        private readonly string _hostName;

        public RabbitMQWorker(
            ISignalRService signalRService,
            IConfiguration configuration,
            IStorageService storageService,
            ILogger<RabbitMQWorker> logger)
        {
            _signalRService = signalRService;
            _config = configuration;
            _storageService = storageService;
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
                    await channel.QueueDeclareAsync(queue: "pdf_result_queue", durable: true, false, false);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var result = JsonSerializer.Deserialize<ScanFileResultDto>(message);

                        if (result == null)
                        {
                            _logger.LogWarning("Failed to deserialize scan result message");
                            return;
                        }

                        await _signalRService.NotifyScanResultAsync(result.UserId, result);
                        await DeleteFileAsync(result.FilePath);
                    };

                    await channel.BasicConsumeAsync(queue: "pdf_result_queue", autoAck: true, consumer: consumer);
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RabbitMQWorker, retrying in 5 seconds...");
                    await Task.Delay(5000, stoppingToken);
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
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "File is busy: {StoragePath}", storagePath);
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
