using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class SendWorkOrderCompletedEmailHandler(
    INotificationService notificationService,
    IAppDbContext context,
    ILogger<SendWorkOrderCompletedEmailHandler> logger
) : INotificationHandler<WorkOrderCompleted>
{
    private readonly INotificationService _notificationService = notificationService;
    private readonly IAppDbContext _context = context;
    private readonly ILogger<SendWorkOrderCompletedEmailHandler> _logger = logger;

    public async Task Handle(WorkOrderCompleted notification, CancellationToken cancellationToken)
    {
        var completedWorkOrder = await _context.WorkOrders
            .Include(wOrder => wOrder.Vehicle!).ThenInclude(veh => veh.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(wOrder => wOrder.Id == notification.WorkOrderId,cancellationToken);

        if(completedWorkOrder is null)
        {
            _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", notification.WorkOrderId);
            
            return;
        }    

        await _notificationService.SendEmailAsync(completedWorkOrder.Vehicle?.Customer?.Email!, cancellationToken);
        await _notificationService.SendSmsAsync(completedWorkOrder.Vehicle!.Customer!.PhoneNumber!, cancellationToken);
    }
}