using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Tests.Common.Billing;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class WorkOrderMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var labor = EmployeeFactory.CreateLabor().Value;
        var part = PartFactory.CreatePart(cost: 100, quantity: 3).Value;
        var repairTasks = RepairTaskFactory.CreateRepairTask(laborCost: 150m, parts: [part]).Value;
        var repairDuration = (int)repairTasks.EstimatedDurationInMins;
        var totalPartsCost = part.Cost * part.Quantity ;
        var totalLaborCost = repairTasks.LaborCost;
        var totalCost = totalLaborCost + totalPartsCost;
        var invoiceLineItem = InvoiceLineItemFactory.CreateInvoiceLineItem(
            lineNumber: 1,
            description: "A description",
            quantity: 1,
            unitPrice: totalCost
        ).Value;
        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: Guid.NewGuid(),
            items: [invoiceLineItem]
        ).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: labor.Id,
            repairTasks: [repairTasks]
        ).Value;

        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;
        workOrder.Invoice = invoice;

        var workOrderDto = workOrder.ToDto();

        Assert.Equal(workOrder.Id, workOrderDto.WorkOrderId);
        Assert.Equal(workOrder.Spot, workOrderDto.Spot);
        Assert.Equal(workOrder.State, workOrderDto.State);
        Assert.Equal(workOrder.StartsAtUtc, workOrderDto.StartsAtUtc);
        Assert.Equal(workOrder.EndsAtUtc, workOrderDto.EndsAtUtc);
        Assert.Equal(workOrder.CreatedAtUtc, workOrderDto.CreatedAt);


        Assert.NotNull(workOrderDto.Labor);
        Assert.Equal(workOrder.LaborId, workOrderDto.Labor!.LaborId);
        Assert.Equal($"{labor.FirstName} {labor.LastName}", workOrderDto.Labor.Name);

        Assert.NotNull(workOrderDto.Vehicle);
        Assert.Equal(vehicle.Id, workOrderDto.Vehicle.VehicleId);
        Assert.Equal(vehicle.Make, workOrderDto.Vehicle.Make);
        Assert.Equal(vehicle.Model, workOrderDto.Vehicle.Model);
        Assert.Equal(vehicle.LicensePlate, workOrderDto.Vehicle.LicensePlate);
        Assert.Equal(vehicle.Year, workOrderDto.Vehicle.Year);

        Assert.Single(workOrderDto.RepairTasks);
        Assert.Equal(totalPartsCost, workOrderDto.TotalPartCost);
        Assert.Equal(totalLaborCost, workOrderDto.TotalLaborCost);
        Assert.Equal(totalCost, workOrderDto.TotalCost);
        Assert.Equal(repairDuration, workOrderDto.TotalDurationInMins);
        Assert.Equal(invoice.Id, workOrderDto.InvoiceId);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var labor = EmployeeFactory.CreateLabor().Value;
        var part = PartFactory.CreatePart(cost: 100, quantity: 3).Value;
        var repairTasks = RepairTaskFactory.CreateRepairTask(laborCost: 150m, parts: [part]).Value;

        var repairDuration = (int)repairTasks.EstimatedDurationInMins;
        var totalPartsCost = part.Quantity * part.Cost;
        var totalLaborCost = repairTasks.LaborCost;
        var totalCost = totalLaborCost + totalPartsCost;
        var invoiceLineItem = InvoiceLineItemFactory.CreateInvoiceLineItem(
            lineNumber: 1,
            description: "A description",
            quantity: 1,
            unitPrice: totalCost
        ).Value;
        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: Guid.NewGuid(),
            items: [invoiceLineItem]
        ).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: labor.Id,
            repairTasks: [repairTasks]
        ).Value;

        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;
        workOrder.Invoice = invoice;

        var workOrders = new List<WorkOrder> {workOrder};

        var workOrdersDtos = workOrders.ToDtos();

        Assert.Single(workOrdersDtos);

        var workOrderDto = workOrdersDtos[0];
        
        Assert.Equal(workOrder.Id, workOrderDto.WorkOrderId);
        Assert.Equal(workOrder.Spot, workOrderDto.Spot);
        Assert.Equal(workOrder.State, workOrderDto.State);
        Assert.Equal(workOrder.StartsAtUtc, workOrderDto.StartsAtUtc);
        Assert.Equal(workOrder.EndsAtUtc, workOrderDto.EndsAtUtc);
        Assert.Equal(workOrder.CreatedAtUtc, workOrderDto.CreatedAt);


        Assert.NotNull(workOrderDto.Labor);
        Assert.Equal(workOrder.LaborId, workOrderDto.Labor!.LaborId);
        Assert.Equal($"{labor.FirstName} {labor.LastName}", workOrderDto.Labor.Name);

        Assert.NotNull(workOrderDto.Vehicle);
        Assert.Equal(vehicle.Id, workOrderDto.Vehicle.VehicleId);
        Assert.Equal(vehicle.Make, workOrderDto.Vehicle.Make);
        Assert.Equal(vehicle.Model, workOrderDto.Vehicle.Model);
        Assert.Equal(vehicle.LicensePlate, workOrderDto.Vehicle.LicensePlate);
        Assert.Equal(vehicle.Year, workOrderDto.Vehicle.Year);

        Assert.Single(workOrderDto.RepairTasks);
        Assert.Equal(totalPartsCost, workOrderDto.TotalPartCost);
        Assert.Equal(totalLaborCost, workOrderDto.TotalLaborCost);
        Assert.Equal(totalCost, workOrderDto.TotalCost);
        Assert.Equal(repairDuration, workOrderDto.TotalDurationInMins);
        Assert.Equal(invoice.Id, workOrderDto.InvoiceId);
    }

    [Fact]
    public void ToListItemDto_ShouldMapSummaryCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var labor = EmployeeFactory.CreateEmployee().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(name: "Oil Change").Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: labor.Id,
            repairTasks: [repairTask]
        ).Value;

        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;

        var workOrderSummaryDto = workOrder.ToListItemDto();

        Assert.Equal(workOrder.Id, workOrderSummaryDto.WorkOrderId);
        Assert.Equal(workOrder.Spot, workOrderSummaryDto.Spot);
        Assert.Equal(workOrder.StartsAtUtc, workOrderSummaryDto.StartsAtUtc);
        Assert.Equal(workOrder.EndsAtUtc, workOrderSummaryDto.EndsAtUtc);
        Assert.Equal(vehicle.Make, workOrderSummaryDto.Vehicle.Make);
        Assert.Equal($"{labor.FirstName} {labor.LastName}", workOrderSummaryDto.Labor);
        Assert.Single(workOrderSummaryDto.RepairTasks);
        Assert.Equal("Oil Change", workOrderSummaryDto.RepairTasks[0]);
        Assert.Equal(workOrder.State, workOrderSummaryDto.State);
    }
}