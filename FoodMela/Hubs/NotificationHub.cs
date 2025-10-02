using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FoodMela.Hubs
{
    public class NotificationHub : Hub
    {
        // Send notification to a specific user
        public async Task SendToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }

        // Broadcast to all connected users (Admin-level notifications)
        public async Task Broadcast(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user.IsInRole("Admin"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            else if (user.IsInRole("Vendor"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Vendors");
            }
            else if (user.IsInRole("Customer"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Customers");
            }

            await base.OnConnectedAsync();
        }

    }
}
