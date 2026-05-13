using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Tests.Common.Customers;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomerById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetCustomerByIdQueryHandlerTests: SubcutaneousTestBase
{
    public GetCustomerByIdQueryHandlerTests(WebAppFactory factory) : base(factory) { }
    
    private async Task<Customer> SeedCustomerAsync(
        string name = "Jane Doe",
        string email = "jane@example.com",
        string phone = "+1234567890")
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(
            name: name, email: email, phoneNumber: phone, vehicles: [vehicle]).Value;

        await _context.Customers.AddAsync(customer, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);
        TrackEntity(customer);

        return customer;
    }

    
    [Fact]
    public async Task Handle_WithExistingCustomer_ShouldReturnDto()
    {
        var customer = await SeedCustomerAsync();
        var getCustomerByIdQuery = new GetCustomerByIdQuery(customer.Id);

        var getCustomerByIdQueryResult = await _mediator.Send(getCustomerByIdQuery, CancellationToken.None);

        Assert.True(getCustomerByIdQueryResult.IsSuccess);
        var dto = getCustomerByIdQueryResult.Value;
        Assert.NotNull(dto);
        Assert.Equal(customer.Id, dto.CustomerId);
        Assert.NotEmpty(dto.Vehicles);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldFail()
    {
        var getCustomerByIdQuery = new GetCustomerByIdQuery(Guid.NewGuid());

        var getCustomerByIdQueryResult = await _mediator.Send(getCustomerByIdQuery, CancellationToken.None);

        Assert.False(getCustomerByIdQueryResult.IsSuccess);
        Assert.Contains(getCustomerByIdQueryResult.Errors, e => e.Code == "Customer_NotFound");
    }
}