using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Queries.GetCustomers;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomersQueryHandlerTests : SubcutaneousTestBase
{
    public GetCustomersQueryHandlerTests(WebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Handle_WhenCustomersExist_ShouldReturnList()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var customer1 = CustomerFactory.CreateCustomer(vehicles: [vehicle1]).Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer2 = CustomerFactory.CreateCustomer(vehicles: [vehicle2]).Value;

        await _context.Vehicles.AddAsync(vehicle1, CancellationToken.None);
        await _context.Vehicles.AddAsync(vehicle2, CancellationToken.None);
        await _context.Customers.AddAsync(customer1, CancellationToken.None);
        await _context.Customers.AddAsync(customer2, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(vehicle1);
        TrackEntity(vehicle2);
        TrackEntity(customer1);
        TrackEntity(customer2);

        var getCustomersQuery = new GetCustomersQuery();

        var getCustomersQueryResult = await _mediator.Send(getCustomersQuery, CancellationToken.None);

        Assert.True(getCustomersQueryResult.IsSuccess);
        Assert.NotNull(getCustomersQueryResult.Value);
        Assert.Equal(4, getCustomersQueryResult.Value.Count); // 2  + 2 seeded
        Assert.Contains(getCustomersQueryResult.Value, c => c.CustomerId == customer1.Id);
        Assert.Contains(getCustomersQueryResult.Value, c => c.CustomerId == customer2.Id);
    }

   
}