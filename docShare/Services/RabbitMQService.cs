using Application.DTOs;
using Application.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace API.Services
{
    public class RabbitMQService
    {
        public async Task SendFileToScan(string filePath, string userId, string documentidto)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: "pdf_scan_queue", durable: true, exclusive: false, autoDelete: false);
            var message = new { FilePath = filePath, UserId = userId, DocIdDto = documentidto };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await channel.BasicPublishAsync(exchange: "", routingKey: "pdf_scan_queue", body: body);
        }
    }
    public class RabbitMQWorker : BackgroundService
    {
        private readonly ISignalRService _signalRService;
        private readonly IConfiguration _config;
        private readonly IStorageService _storageService;
        public RabbitMQWorker(ISignalRService signalRService, IConfiguration configuration, IStorageService storageService)
        {
            _signalRService = signalRService;
            _config = configuration;
            _storageService = storageService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: "pdf_result_queue", durable: true, false, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var result = JsonSerializer.Deserialize<ScanFileResultDto>(message);
                await _signalRService.NotifyScanResultAsync(result.UserId, result);
                await DeleteFile(result.FilePath);
            };

            await channel.BasicConsumeAsync(queue: "pdf_result_queue", autoAck: true, consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        protected async Task<bool> DeleteFile(string storagePath)
        {
            try
            {
                if (await _storageService.FileExistsAsync(storagePath))
                {
                    await _storageService.DeleteFileAsync(storagePath);
                    return true;
                }
                else
                {
                    return false; 
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"File đang bận: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi không xác định: {ex.Message}");
                return false;
            }
        }
    }
}
