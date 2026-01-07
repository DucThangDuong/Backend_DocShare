using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs;
namespace API.Services
{
    public class RabbitMQService
    {
        public async Task SendFileToScan(string filePath, string signalRConnectionID, string documentidto)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: "pdf_scan_queue", durable: true, exclusive: false, autoDelete: false);

            var message = new { FilePath = filePath, UserId = signalRConnectionID, DocIdDto = documentidto };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await channel.BasicPublishAsync(exchange: "", routingKey: "pdf_scan_queue", body: body);
        }
    }
    public class RabbitMQWorker : BackgroundService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConfiguration _config;
        public RabbitMQWorker(IHubContext<NotificationHub> hubContext, IConfiguration configuration)
        {
            _hubContext = hubContext;
            _config = configuration;
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
                await _hubContext.Clients.Client(result.UserId).SendAsync("ReceiveScanResult", new { result.Status, result.DocIdDto, result.Message });
                DeleteFile(result.FilePath);
            };

            await channel.BasicConsumeAsync(queue: "pdf_result_queue", autoAck: true, consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        protected bool DeleteFile(string storagePath)
        {
            try
            {
                if (File.Exists(storagePath))
                {
                    File.Delete(storagePath);
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
