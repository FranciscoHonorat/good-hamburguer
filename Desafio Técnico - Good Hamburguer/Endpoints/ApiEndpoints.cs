using Desafio_Técnico___Good_Hamburguer.Contracts;
using Desafio_Técnico___Good_Hamburguer.Services;

namespace Desafio_Técnico___Good_Hamburguer.Endpoints;

public static class ApiEndpoints
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
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

        return app;
    }
}
