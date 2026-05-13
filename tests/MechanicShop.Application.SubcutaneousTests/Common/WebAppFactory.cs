using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using MechanicShop.Api;
using MechanicShop.Application.Common.Interfaces;
using Testcontainers.MsSql;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MechanicShop.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MechanicShop.Infrastructure.BackgroundJobs;
using Microsoft.EntityFrameworkCore;
using MechanicShop.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MechanicShop.Tests.Common.Billing;

namespace MechanicShop.Application.SubcutaneousTests.Common;


public class WebAppFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder(
        "mcr.microsoft.com/mssql/server:2022-latest").Build();

    public IMediator CreateMediator()
    {
        var serviceScope = Services.CreateScope();
        return serviceScope.ServiceProvider.GetRequiredService<IMediator>();
    }
    public IAppDbContext CreateAppDbContext()
    {
        var serviceScope = Services.CreateScope();
        return serviceScope.ServiceProvider.GetRequiredService<IAppDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var serviceScope = Services.CreateScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.WorkOrders.RemoveRange(context.WorkOrders);
        await context.SaveChangesAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => _dbContainer.StopAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<OverdueBookingCleanupService>();
            services.RemoveAll<AppSettings>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            services.PostConfigure<AppSettings>(options =>
            {
               options.OpeningTime = new TimeOnly(9, 0); 
               options.ClosingTime = new TimeOnly(18, 0); 
            });
            
            services.RemoveAll<IInvoicePdfGenerator>();
            services.AddScoped<IInvoicePdfGenerator, FakeInvoicePdfGenerator>();
        });
    }

    
}