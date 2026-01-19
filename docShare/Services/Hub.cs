using Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
namespace API.Services
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            System.Console.WriteLine($"---  SignalR: User '{userId}' đã kết nối với ID {Context.ConnectionId} ---");

            await base.OnConnectedAsync();
        }
    }
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var claims = connection.User?.Claims;
            if (claims == null || !claims.Any())
            {
                return null;
            }
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
