using System.Text;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Data.Interceptors;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Infrastructure.Identity.Policies;
using MechanicShop.Infrastructure.RealTime;
using MechanicShop.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
namespace MechanicShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);            
        });
    
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<ApplicationDbContextInitializer>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("JwtSettings");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
            };
        });


        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 1;
            options.SignIn.RequireConfirmedAccount = false;
        }
        ).AddRoles<IdentityRole>()
         .AddEntityFrameworkStores<AppDbContext>();


        services.AddScoped<IAuthorizationHandler, LaborAssignedHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"))
            .AddPolicy("AssignedLaborOnly", 
                policy => policy.Requirements.Add(new LaborAssignedRequirement()));
 
        services.AddTransient<IIdentityService, IdentityService>();
 
        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10),
            LocalCacheExpiration = TimeSpan.FromSeconds(30)
        });

        services.AddScoped<IWorkOrderPolicy, WorkOrderPolicy>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IInvoicePdfGenerator, InvoicePDFGeneratorService>();
        services.AddScoped<INotificationService,NotificationService>();
         services.AddScoped<IWorkOrderNotifier, SignalWorkOrderNotifier>();

        services.AddHostedService<OverdueBookingCleanupService>();

        return services;
    }
}