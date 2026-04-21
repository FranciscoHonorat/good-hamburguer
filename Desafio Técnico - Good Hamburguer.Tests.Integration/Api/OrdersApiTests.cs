using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Desafio_Técnico___Good_Hamburguer.Tests.Integration.Api;

public sealed class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetMenu_ShouldReturnConfiguredItems()
    {
        var response = await _client.GetAsync("/api/menu");

        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(items);
        Assert.Equal(5, items.Count);
    }

    [Fact]
    public async Task CreateOrder_WithDuplicateItems_ShouldReturnBadRequestFromMiddleware()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            itemCodes = new[] { "SODA", "SODA" }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var message = payload.TryGetProperty("detail", out var detail)
            ? detail.GetString()
            : payload.TryGetProperty("message", out var camelCaseMessage)
                ? camelCaseMessage.GetString()
                : payload.GetProperty("Message").GetString();

        Assert.Contains("duplicados", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OrdersCrud_ShouldCreateUpdateAndDeleteOrder()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/orders", new
        {
            itemCodes = new[] { "XBURGER", "SODA" }
        });

        createResponse.EnsureSuccessStatusCode();
        Assert.NotNull(createResponse.Headers.Location);

        var createdPayload = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = createdPayload.GetProperty("id").GetGuid();

        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
        getResponse.EnsureSuccessStatusCode();

        var updateResponse = await _client.PutAsJsonAsync($"/api/orders/{orderId}", new
        {
            itemCodes = new[] { "XBURGER", "FRIES", "SODA" }
        });
        updateResponse.EnsureSuccessStatusCode();

        var deleteResponse = await _client.DeleteAsync($"/api/orders/{orderId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeletedResponse = await _client.GetAsync($"/api/orders/{orderId}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }

    [Fact]
    public async Task GetOrders_ShouldReturnPaginatedResponse()
    {
        for (var i = 0; i < 3; i++)
        {
            var createResponse = await _client.PostAsJsonAsync("/api/orders", new
            {
                itemCodes = new[] { "XBURGER" }
            });

            createResponse.EnsureSuccessStatusCode();
        }

        var response = await _client.GetAsync("/api/orders?page=1&pageSize=2");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, payload.GetProperty("page").GetInt32());
        Assert.Equal(2, payload.GetProperty("pageSize").GetInt32());
        Assert.True(payload.GetProperty("totalItems").GetInt32() >= 3);
        Assert.Equal(2, payload.GetProperty("items").GetArrayLength());
    }
}
