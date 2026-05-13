using System;
using System.Collections.Generic;

namespace MechanicShop.Client.Models;


public class InvoiceModel
{
    public Guid InvoiceId {get; set; }
    public Guid WorkOrderId {get; set; }
    public DateTimeOffset IssuedAtUtc {get; set; }
    public CustomerModel? Customer {get; set; }
    public VehicleModel? Vehicle {get; set; } 
    public decimal? DiscountAmount {get; set; }
    public decimal TaxAmount {get; set; }
    public decimal SubTotal {get; set; }
    public decimal Total {get; set; }
    public string? PaymentStatus {get; set; }
    public List<InvoiceLineItemModel> Items {get; set; } = [];
}