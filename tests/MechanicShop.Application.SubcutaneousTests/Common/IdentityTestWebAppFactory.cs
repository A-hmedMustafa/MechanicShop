using MechanicShop.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Common;

public class IdentityTestWebAppFactory: WebAppFactory, IAsyncLifetime
{
    public readonly IIdentityService FakeIdentityService = Substitute.For<IIdentityService>();
    public readonly ITokenProvider FakeTokenProvider = Substitute.For<ITokenProvider>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IIdentityService>();
            services.AddSingleton(FakeIdentityService);

            services.RemoveAll<ITokenProvider>();
            services.AddSingleton(FakeTokenProvider);
        });
    }

}