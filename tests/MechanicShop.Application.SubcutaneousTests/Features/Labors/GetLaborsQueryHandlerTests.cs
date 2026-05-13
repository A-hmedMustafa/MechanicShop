using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Queries.GetLabors;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Xunit;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetLaborsQueryHandlerTests : SubcutaneousTestBase
{
    public GetLaborsQueryHandlerTests(WebAppFactory factory) : base(factory) { }


    [Fact]
    public async Task Handle_WhenLaborEmployeesExist_ShouldReturnList()
    {
      
        var labor1 = EmployeeFactory.CreateLabor().Value;
        var labor2 = EmployeeFactory.CreateLabor().Value;
        var manager = EmployeeFactory.CreateManager().Value;

     
        await _context.Employees.AddAsync(labor1, CancellationToken.None);
        await _context.Employees.AddAsync(labor2, CancellationToken.None);
        await _context.Employees.AddAsync(manager, CancellationToken.None);
        await _context.SaveChangesAsync(CancellationToken.None);

        TrackEntity(labor1);
        TrackEntity(labor2);
        TrackEntity(manager);

        var getLaborsQuery = new GetLaborsQuery();

        
        var getLaborsQueryResult = await _mediator.Send(getLaborsQuery, CancellationToken.None);

        
        Assert.True(getLaborsQueryResult.IsSuccess);
        Assert.NotNull(getLaborsQueryResult.Value);
        Assert.Equal(6, getLaborsQueryResult.Value.Count); // 2 + 4 Test Users
        Assert.Contains(getLaborsQueryResult.Value, l => l.LaborId == labor1.Id);
        Assert.Contains(getLaborsQueryResult.Value, l => l.LaborId == labor2.Id);
        Assert.DoesNotContain(getLaborsQueryResult.Value, l => l.LaborId == manager.Id);
    }
}