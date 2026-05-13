using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public sealed class Invoice : AuditableEntity
{
    public Guid WorkOrderId {get;}
    public decimal SubTotal => LineItems.Sum(line=> line.LineTotal);
    public decimal TaxAmount {get;}
    public decimal DiscountAmount {get; private set;}
    public decimal Total => SubTotal + TaxAmount - DiscountAmount;
    public DateTimeOffset IssuedAtUtc {get;}
    public DateTimeOffset? PaidAtUtc {get; private set;}
    private readonly List<InvoiceLineItem> _lineItems = [];
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();
    public WorkOrder? WorkOrder {get; set;}
    public InvoiceStatus Status {get; private set;}

    private Invoice() {} 
    private Invoice(
        Guid id, 
        Guid workOrderId, 
        DateTimeOffset issuedAtUtc,
        List<InvoiceLineItem> lineItems,
        decimal discountAmount,
        decimal taxAmount):base(id)
    {
        WorkOrderId = workOrderId;
        IssuedAtUtc = issuedAtUtc;
        DiscountAmount = discountAmount;
        _lineItems = lineItems;
        TaxAmount = taxAmount;
        Status = InvoiceStatus.Unpaid;
    }

    public static Result<Invoice> Create(
        Guid id,
        Guid workOrderId,
        decimal discountAmount,
        decimal taxAmount,
        List<InvoiceLineItem> lineItems,
        TimeProvider datetime
    )
    {
         if (workOrderId == Guid.Empty)
        {
            return InvoiceErrors.WorkOrderIdInvalid;
        }

        if (lineItems is null || lineItems.Count == 0)
        {
            return InvoiceErrors.LineItemsEmpty;
        }

        return new Invoice(id, workOrderId, datetime.GetUtcNow(), lineItems, discountAmount, taxAmount);
    }

    public Result<Updated> ApplyDiscount(decimal discountAmount)
    {
        if(Status != InvoiceStatus.Unpaid)
        {
            return InvoiceErrors.InvoiceLocked;
        }
        if (discountAmount < 0)
        {
            return InvoiceErrors.DiscountNegative;
        }
        if(discountAmount > SubTotal)
        {
            return InvoiceErrors.DiscountExceedsSubtotal;
        }

        DiscountAmount = discountAmount;

        return Result.Updated;
    }
    public Result<Updated> MarkAsPaid(TimeProvider timeProvider)
    {
        if (Status != InvoiceStatus.Unpaid)
        {
            return InvoiceErrors.InvoiceLocked;
        }

        Status = InvoiceStatus.Paid;
        PaidAtUtc = timeProvider.GetUtcNow();

        return Result.Updated;
    }
}