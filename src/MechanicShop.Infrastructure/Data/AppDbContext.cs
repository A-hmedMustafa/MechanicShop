using System.Runtime.CompilerServices;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Emloyees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data;


public class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator) : IdentityDbContext<AppUser>(options), IAppDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Part> Parts => Set<Part>();

    public DbSet<Vehicle> Vehicles =>Set<Vehicle>();

    public DbSet<RepairTask> RepairTasks => Set<RepairTask>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entities = ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is Entity domainLayerEntity && domainLayerEntity.DomainEvents.Count != 0)
            .Select(entry => (Entity)entry.Entity)
            .ToList();  

        var domainEvents = entities
            .SelectMany(entities => entities.DomainEvents)
            .ToList();    

        foreach(var ev in domainEvents)
        {
            await mediator.Publish(ev, cancellationToken);
        }    

        foreach(var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}