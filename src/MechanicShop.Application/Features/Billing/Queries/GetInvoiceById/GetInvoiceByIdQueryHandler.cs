using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler(
    IAppDbContext context,
    ILogger<GetInvoiceByIdQueryHandler> logger
) : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<GetInvoiceByIdQueryHandler> _logger = logger;

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(inv => inv.WorkOrder!).ThenInclude(wOrder => wOrder.Vehicle!).ThenInclude(rTask => rTask.Customer)
            .Include(inv => inv.LineItems)
            .FirstOrDefaultAsync(inv => inv.Id == request.InvoiceId, cancellationToken);
        if (invoice is null)
        {
            _logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
            return Error.NotFound("Invoice_Not_Found");
        }

        return invoice.ToDto();
    }
}