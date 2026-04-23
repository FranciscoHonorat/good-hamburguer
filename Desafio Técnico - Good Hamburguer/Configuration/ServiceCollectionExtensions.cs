using Desafio_Técnico___Good_Hamburguer.Data;
using Desafio_Técnico___Good_Hamburguer.Frontend.Services;
using Desafio_Técnico___Good_Hamburguer.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

namespace Desafio_Técnico___Good_Hamburguer.Configuration;

public static class ServiceCollectionExtensions
{
    public const string CorsPolicyName = "ApiCors";

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        ValidateProductionSettings(builder);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddScoped(sp =>
        {
            var navigationManager = sp.GetRequiredService<NavigationManager>();
            return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
        });

        builder.Services.AddScoped<ApiErrorReader>();
        builder.Services.AddScoped<MenuApiClient>();
        builder.Services.AddScoped<OrdersApiClient>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<MenuService>();
        builder.Services.AddScoped<OrderService>();

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseInMemoryDatabase("GoodHamburgerDb");
                return;
            }

            options.UseNpgsql(connectionString);
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

                if (allowedOrigins.Length == 0)
                {
                    return;
                }

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Muitas requisições. Tente novamente em instantes." }, cancellationToken);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: "api-global",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });

        return builder;
    }

    private static void ValidateProductionSettings(WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsProduction())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(builder.Configuration["Security:ApiKey"]))
        {
            throw new InvalidOperationException("A configuração 'Security:ApiKey' é obrigatória em produção.");
        }

        if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
        {
            throw new InvalidOperationException("A configuração 'ConnectionStrings:DefaultConnection' é obrigatória em produção.");
        }
    }
}
