using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MechanicShop.Infrastructure.RealTime;

public sealed class SignalWorkOrderNotifier(
    IHubContext<WorkOrderHub> hubContext
) : IWorkOrderNotifier
{
    private readonly IHubContext<WorkOrderHub> _hubContext = hubContext;

    public Task NotifyWorkOrdersChangedAsync(CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync("WorkOrdersChanged", cancellationToken);
    }
}
