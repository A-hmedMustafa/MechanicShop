using MechanicShop.Application.Common;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Emloyees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Common;
[Collection(WebAppFactoryCollection.CollectionName)]
public abstract class SubcutaneousTestBase : IAsyncLifetime
{
    private readonly WebAppFactory _factory;
    private readonly List<object> _entitiesToDelete = new();

    protected IMediator _mediator { get; private set; }
    protected IAppDbContext _context { get; private set; }

    protected SubcutaneousTestBase(WebAppFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        _mediator = _factory.CreateMediator();
        _context = _factory.CreateAppDbContext();
        return Task.CompletedTask;
    }

    protected void TrackEntity(object entity) => _entitiesToDelete.Add(entity);

    public async Task DisposeAsync()
    {
        await _context.Invoices.ExecuteDeleteAsync();
        await _context.WorkOrders.ExecuteDeleteAsync();

        foreach (var entity in _entitiesToDelete.AsEnumerable().Reverse())
        {
            switch (entity)
            {
                case WorkOrder wo:
                    await _context.WorkOrders.Where(x => x.Id == wo.Id).ExecuteDeleteAsync();
                    break;
                case Customer c:
                    await _context.Customers.Where(x => x.Id == c.Id).ExecuteDeleteAsync();
                    break;
                case Vehicle v:
                    await _context.Vehicles.Where(x => x.Id == v.Id).ExecuteDeleteAsync();
                    break;
                case RepairTask rt:
                    await _context.RepairTasks.Where(x => x.Id == rt.Id).ExecuteDeleteAsync();
                    break;
                case Employee e:
                    await _context.Employees.Where(x => x.Id == e.Id).ExecuteDeleteAsync();
                    break;
            }
        }
    }
}