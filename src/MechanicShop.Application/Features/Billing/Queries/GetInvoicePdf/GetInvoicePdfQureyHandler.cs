using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public class GetInvoicePdfQueryHandler(
    ILogger<GetInvoicePdfQueryHandler> logger,
    IInvoicePdfGenerator pdfGenerator,
    IAppDbContext context
) : IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
    private readonly ILogger<GetInvoicePdfQueryHandler> _logger = logger;
    private readonly IInvoicePdfGenerator _pdfGenerator = pdfGenerator;
    private readonly IAppDbContext _context = context;

    public async Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(inv => inv.LineItems)
            .FirstOrDefaultAsync(inv => inv.Id == request.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
            
            return Error.NotFound("Invoice_Not_Found");
        }

        try
        {
            var pdfContent = _pdfGenerator.Generate(invoice);

            var invoicePdf = new InvoicePdfDto
            {
                Content = pdfContent,
                FileName = $"invoice_{invoice.Id}.pdf"
            };

            return invoicePdf;
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for InvoiceId: {InvoiceId}", request.InvoiceId);

            return Error.Failure("An error occurred while generating the invoice PDF.");
        }

    }
}