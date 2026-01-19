using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace API.Services
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<NotificationHub> _hubcontext;
        public SignalRService(IHubContext<NotificationHub> hubcontext)
        {
            _hubcontext = hubcontext;
        }
        public async Task NotifyScanResultAsync(string userId, ScanFileResultDto result)
        {
            await _hubcontext.Clients.User(userId).SendAsync("ReceiveScanResult", new
            {
                result.Status,
                result.DocIdDto,
                result.Message
            });
        }
    }
}
