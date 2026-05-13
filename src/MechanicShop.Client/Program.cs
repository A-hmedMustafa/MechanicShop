using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MechanicShop.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using MechanicShop.Client.Services;
using MechanicShop.Client.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthProvider>();

builder.Services.AddScoped(sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>() );
builder.Services.AddHttpClient("MechanicShopClient",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
).AddHttpMessageHandler<BearerTokenHandler>();;
builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
