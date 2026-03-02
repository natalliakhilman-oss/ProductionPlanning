

using Microsoft.AspNetCore.SignalR;

namespace ProductionPlanning.Hubs
{
    public class RequestHub : Hub
    {
        public async Task UpdateRequestCount(int count)
        {
            await Clients.All.SendAsync("ReceiveRequestCount", count);
        }
        public async Task UpdateOrderCount(int count)
        {
            await Clients.All.SendAsync("ReceiveOrderCount", count);
        }
    }
}
