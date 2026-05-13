using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.BackgroundJobs;

public class OverdueBookingCleanupService(
    ILogger<OverdueBookingCleanupService> logger,
    IOptions<AppSettings> options,
    IServiceScopeFactory scopeFactory,
    TimeProvider dateTime
) : BackgroundService
{
    private readonly ILogger<OverdueBookingCleanupService> _logger = logger;
    private readonly AppSettings _appSettings = options.Value;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly TimeProvider _dateTime = dateTime;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_appSettings.CleanupJobIntervalInMinutes));

        while(await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation("Checking overdue work orders at {Now}", _dateTime.GetUtcNow());

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                var cutOff = _dateTime.GetUtcNow().AddMinutes(-_appSettings.CancellationDeadlineInMinutes);

                var overdueWorkOrders = await context.WorkOrders
                    .Where(wOrder => wOrder.State == WorkOrderState.Scheduled 
                        && wOrder.StartsAtUtc <= cutOff)
                    .ToListAsync(stoppingToken);

                if(overdueWorkOrders.Count > 0)
                {
                    foreach(var wORder in overdueWorkOrders)
                    {
                        var workOrderCancellationResult = wORder.Cancel();

                        if(workOrderCancellationResult.IsError)
                        {
                            _logger.LogWarning("Failed to cancel WorkOrder {Id} : {Error}", 
                                wORder.Id, workOrderCancellationResult.Errors); 
                        }
                    }
                await context.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Canceled {Count} overdue Work Orders: {Ids}.",
                     overdueWorkOrders.Count, overdueWorkOrders.Select(wOrder => wOrder.Id));
                }
                else
                {
                    _logger.LogInformation("No overdue work orders found.");
                }

           
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in cleaning up overdue work orders.");
            }
            
        }
    }
}