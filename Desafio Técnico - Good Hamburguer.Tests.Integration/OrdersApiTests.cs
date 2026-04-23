using System.Net;
using System.Net.Http.Json;
using Desafio_Técnico___Good_Hamburguer.Contracts;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Desafio_Técnico___Good_Hamburguer.Tests.Integration;

public sealed class OrdersApiTests
{
    [Fact]
    public async Task GetMenu_DeveRetornarItensDoCardapio()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/menu");
        var items = await response.Content.ReadFromJsonAsync<List<MenuItemResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(items);
        Assert.NotEmpty(items!);
    }

    [Fact]
    public async Task PostOrders_ComComboCompleto_DeveCriarPedidoComDesconto()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new UpsertOrderRequest
        {
            ItemCodes = ["XBURGER", "FRIES", "SODA"]
        });

        var payload = await response.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(0.20m, payload!.DiscountPercentage);
        Assert.Equal(7.60m, payload.Total);
    }

    [Fact]
    public async Task PostOrders_ComItensDaMesmaCategoria_DeveRetornarBadRequestPadronizado()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new UpsertOrderRequest
        {
            ItemCodes = ["XBURGER", "XEGG"]
        });

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem!.Status);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Orders_CrudBasico_DevePermitirCriarConsultarEExcluir()
    {
        await using var factory = new ApiTestFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/orders", new UpsertOrderRequest
        {
            ItemCodes = ["XBURGER", "SODA"]
        });
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/orders/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await client.GetAsync($"/api/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }
}
