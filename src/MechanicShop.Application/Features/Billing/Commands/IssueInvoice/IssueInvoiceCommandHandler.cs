using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Constants;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;



public class IssueInvoiceCommandHandler(
    ILogger<IssueInvoiceCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    TimeProvider datetime
    )
    : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
    private readonly ILogger<IssueInvoiceCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _datetime = datetime;
    private readonly HybridCache _cache = cache;

    public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .Include(wOrder => wOrder.RepairTasks).ThenInclude(rTask => rTask.Parts)
            .Include(wOrder => wOrder.Vehicle!).ThenInclude(wOrder => wOrder.Customer)
            .FirstOrDefaultAsync(wOrder => wOrder.Id == request.WorkOrderId,
                cancellationToken);

        if (workOrder is null)
        {
            _logger.LogWarning("Invoice issuance failed. WorkOrder {WorkOrderId} not found.", request.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }      

        
        if (workOrder.State != WorkOrderState.Completed)
        {
            _logger.LogWarning("Invoice issuance rejected. WorkOrder {WorkOrderId} is not in completed.", request.WorkOrderId);

            return ApplicationErrors.WorkOrderMustBeCompletedForInvoicing;
        }

        Guid invoiceId = Guid.NewGuid();

        var lineItems = new List<InvoiceLineItem>();

        var lineNumber = 1;


        foreach(var (repairTask, taskIndex) in workOrder.RepairTasks.Select((rTask, tIndex) => (rTask, tIndex + 1)))
        {
            var partsSummary = repairTask.Parts.Any() 
                ? string.Join(Environment.NewLine, repairTask.Parts
                    .Select(p => $"    • {p.Name} x {p.Quantity} @ {p.Cost:C}")) 
                :"   • No Parts";

            var lineDescription = 
                $"{taskIndex}: {repairTask.Name}{Environment.NewLine}"   +
                $"Labor = {repairTask.LaborCost:C}{Environment.NewLine}" +
                $"Parts:{Environment.NewLine}{partsSummary}";

            var totalPartsCost = repairTask.Parts.Sum(p => p.Cost * p.Quantity);
            var totalTaskCost = totalPartsCost + repairTask.LaborCost;

            var createLineItemResult = InvoiceLineItem.Create(
                invoiceId,
                lineNumber++,
                lineDescription,
                1,
                totalTaskCost
            );

            if (createLineItemResult.IsError)
            {
                return createLineItemResult.Errors;
            }

            lineItems.Add(createLineItemResult.Value);
        }

        var subTotal = lineItems.Sum(lItem => lItem.LineTotal);

        var taxAmount = subTotal * MechanicShopConstants.TaxRate;

        var discountAmount = workOrder.Discount ?? 0m;

        var createInvoiceResult = Invoice.Create(
            id: invoiceId,
            workOrderId: workOrder.Id,
            lineItems: lineItems,
            discountAmount: discountAmount,
            taxAmount: taxAmount,
            datetime: _datetime
        );

        
        if (createInvoiceResult.IsError)
        {
            _logger.LogWarning(
                 "Invoice creation failed for WorkOrderId: {WorkOrderId}. Errors: {@Errors}",
                 request.WorkOrderId,
                 createInvoiceResult.Errors);

            return createInvoiceResult.Errors;
        }

        var invoice = createInvoiceResult.Value;

        await _context.Invoices.AddAsync(invoice, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("invoice", cancellationToken);

        _logger.LogInformation("Invoice {InvoiceId} issued for WorkOrder {WorkOrderId}.", invoice.Id, workOrder.Id);  

        return invoice.ToDto();
    }

}