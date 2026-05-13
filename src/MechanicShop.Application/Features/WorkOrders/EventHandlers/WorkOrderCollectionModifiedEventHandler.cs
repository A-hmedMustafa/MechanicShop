using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class WorkOrderCollectionModifiedEventHandler(
    IWorkOrderNotifier notifier) : INotificationHandler<WorkOrderCollectionModified>

{
    private readonly IWorkOrderNotifier _notifier = notifier;

    public async Task Handle(WorkOrderCollectionModified notification, CancellationToken cancellationToken)
    {
       await _notifier.NotifyWorkOrdersChangedAsync(cancellationToken);
    }
}