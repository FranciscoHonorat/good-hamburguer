using Desafio_Técnico___Good_Hamburguer.Components;
using Desafio_Técnico___Good_Hamburguer.Contracts;
using Desafio_Técnico___Good_Hamburguer.Data;
using Desafio_Técnico___Good_Hamburguer.Frontend.Services;
using Desafio_Técnico___Good_Hamburguer.Middleware;
using Desafio_Técnico___Good_Hamburguer.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "ApiCors";
const long MaxApiPayloadSizeBytes = 32 * 1024;

if (builder.Environment.IsProduction())
{
    if (string.IsNullOrWhiteSpace(builder.Configuration["Security:ApiKey"]))
    {
        throw new InvalidOperationException("A configuração 'Security:ApiKey' é obrigatória em produção.");
    }

    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
    {
        throw new InvalidOperationException("A configuração 'ConnectionStrings:DefaultConnection' é obrigatória em produção.");
    }
}

// Add services to the container.
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicyName);

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) &&
        context.Request.ContentLength is > MaxApiPayloadSizeBytes)
    {
        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Payload muito grande.",
            Detail = $"O tamanho máximo permitido para requisições da API é de {MaxApiPayloadSizeBytes} bytes.",
            Status = StatusCodes.Status413PayloadTooLarge,
            Instance = context.Request.Path
        });
        return;
    }

    await next();
});

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseRateLimiter();
app.UseAntiforgery();

var api = app.MapGroup("/api");

api.MapGet("/menu", (MenuService menuService) => Results.Ok(menuService.GetAll().Select(x => x.ToResponse())));

var orders = api.MapGroup("/orders");

orders.MapGet("", async (OrderService orderService, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
{
    var result = await orderService.GetAllAsync(page, pageSize, cancellationToken);
    return Results.Ok(result.ToResponse());
});

orders.MapGet("/{id:guid}", async (Guid id, OrderService orderService, CancellationToken cancellationToken) =>
{
    var order = await orderService.GetByIdOrThrowAsync(id, cancellationToken);
    return Results.Ok(order.ToResponse());
})
.WithName("GetOrderById");

orders.MapPost("", async (UpsertOrderRequest request, OrderService orderService, CancellationToken cancellationToken) =>
{
    var order = await orderService.CreateAsync(request, cancellationToken);
    return Results.CreatedAtRoute("GetOrderById", new { id = order.Id }, order.ToResponse());
});

orders.MapPut("/{id:guid}", async (Guid id, UpsertOrderRequest request, OrderService orderService, CancellationToken cancellationToken) =>
{
    var order = await orderService.UpdateAsync(id, request, cancellationToken);
    return Results.Ok(order.ToResponse());
});

orders.MapDelete("/{id:guid}", async (Guid id, OrderService orderService, CancellationToken cancellationToken) =>
{
    await orderService.DeleteAsync(id, cancellationToken);
    return Results.NoContent();
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;
