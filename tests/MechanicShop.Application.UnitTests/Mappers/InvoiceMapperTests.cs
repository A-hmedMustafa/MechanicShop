using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.Billing;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class InvoiceMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id).Value;
        var lineItem = InvoiceLineItemFactory.CreateInvoiceLineItem().Value;
        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: workOrder.Id,
            items: [lineItem]
            ).Value;

        invoice.WorkOrder = workOrder;
        workOrder.Vehicle = vehicle;
        typeof(Vehicle).GetProperty("Customer")?.SetValue(vehicle, customer);
        var invoiceDto = invoice.ToDto();

        Assert.Equal(invoice.Id, invoiceDto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, invoiceDto.WorkOrderId);
        Assert.Equal(invoice.IssuedAtUtc, invoiceDto.IssuedAtUtc);
        Assert.Equal(invoice.DiscountAmount, invoiceDto.DiscountAmount);
        Assert.Equal(invoice.TaxAmount, invoiceDto.TaxAmount);
        Assert.Equal(invoice.SubTotal, invoiceDto.SubTotal);
        Assert.Equal(invoice.Total, invoiceDto.Total);
        Assert.Single(invoiceDto.Items);


        var customerDto = invoiceDto.Customer!;
        Assert.NotNull(invoiceDto.Customer);
        Assert.Equal(customer.Id, customerDto.CustomerId);
        Assert.Equal(customer.Name, customerDto.Name);
        Assert.Equal(customer.Email, customerDto.Email);
        Assert.Equal(customer.PhoneNumber, customerDto.PhoneNumber);
        
        var vehicleDto = invoiceDto.Vehicle!;
        Assert.NotNull(invoiceDto.Vehicle);
        Assert.Equal(vehicle.Id, vehicleDto.VehicleId);
        Assert.Equal(vehicle.Make, vehicleDto.Make);
        Assert.Equal(vehicle.Model, vehicleDto.Model);
        Assert.Equal(vehicle.Year, vehicleDto.Year);
        Assert.Equal(vehicle.LicensePlate, vehicleDto.LicensePlate);

        var itemDto =  invoiceDto.Items[0];
        Assert.Equal(lineItem.InvoiceId, itemDto.InvoiceId);
        Assert.Equal(lineItem.Description, itemDto.Description);
        Assert.Equal(lineItem.Quantity, itemDto.Quantity);
        Assert.Equal(lineItem.UnitPrice, itemDto.UnitPrice);
        Assert.Equal(lineItem.LineNumber, itemDto.LineNumber);

    }

    [Fact]
    public void ToDtos_ShouldMapCorrectly()
    {  
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle]).Value;
        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id).Value;
        var lineItem = InvoiceLineItemFactory.CreateInvoiceLineItem().Value;
        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: workOrder.Id,
            items: [lineItem]
            ).Value;
        invoice.WorkOrder = workOrder;
        workOrder.Vehicle = vehicle;
        typeof(Vehicle).GetProperty("Customer")?.SetValue(vehicle, customer);
        var invoices = new List<Invoice> {invoice};

        var invoicesDtos = invoices.ToDtos();

        Assert.Single(invoicesDtos);
        var invoiceDto = invoicesDtos[0];

        Assert.Equal(invoice.Id, invoiceDto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, invoiceDto.WorkOrderId);
        Assert.Equal(invoice.IssuedAtUtc, invoiceDto.IssuedAtUtc);
        Assert.Equal(invoice.DiscountAmount, invoiceDto.DiscountAmount);
        Assert.Equal(invoice.TaxAmount, invoiceDto.TaxAmount);
        Assert.Equal(invoice.SubTotal, invoiceDto.SubTotal);
        Assert.Equal(invoice.Total, invoiceDto.Total);
        Assert.Single(invoiceDto.Items);


        var customerDto = invoiceDto.Customer!;
        Assert.NotNull(invoiceDto.Customer);
        Assert.Equal(customer.Id, customerDto.CustomerId);
        Assert.Equal(customer.Name, customerDto.Name);
        Assert.Equal(customer.Email, customerDto.Email);
        Assert.Equal(customer.PhoneNumber, customerDto.PhoneNumber);
        
        var vehicleDto = invoiceDto.Vehicle!;
        Assert.NotNull(invoiceDto.Vehicle);
        Assert.Equal(vehicle.Id, vehicleDto.VehicleId);
        Assert.Equal(vehicle.Make, vehicleDto.Make);
        Assert.Equal(vehicle.Model, vehicleDto.Model);
        Assert.Equal(vehicle.Year, vehicleDto.Year);
        Assert.Equal(vehicle.LicensePlate, vehicleDto.LicensePlate);

        var itemDto =  invoiceDto.Items[0];
        Assert.Equal(lineItem.InvoiceId, itemDto.InvoiceId);
        Assert.Equal(lineItem.Description, itemDto.Description);
        Assert.Equal(lineItem.Quantity, itemDto.Quantity);
        Assert.Equal(lineItem.UnitPrice, itemDto.UnitPrice);
        Assert.Equal(lineItem.LineNumber, itemDto.LineNumber);
}}