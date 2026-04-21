using Desafio_Técnico___Good_Hamburguer.Contracts;
using System.Net.Http.Json;

namespace Desafio_Técnico___Good_Hamburguer.Frontend.Services;

public sealed class OrdersApiClient(HttpClient httpClient, ApiErrorReader apiErrorReader)
{
    public async Task<PaginatedResponse<OrderResponse>> GetOrdersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/orders?page={page}&pageSize={pageSize}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await apiErrorReader.ReadErrorMessageAsync(response));
        }

        var payload = await response.Content.ReadFromJsonAsync<PaginatedResponse<OrderResponse>>(cancellationToken);
        return payload ?? new PaginatedResponse<OrderResponse>
        {
            Items = [],
            Page = page,
            PageSize = pageSize,
            TotalItems = 0,
            TotalPages = 0
        };
    }

    public async Task<OrderResponse> CreateOrderAsync(UpsertOrderRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/orders", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await apiErrorReader.ReadErrorMessageAsync(response));
        }

        return await ReadOrderOrThrowAsync(response, cancellationToken);
    }

    public async Task<OrderResponse> UpdateOrderAsync(Guid id, UpsertOrderRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/orders/{id}", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await apiErrorReader.ReadErrorMessageAsync(response));
        }

        return await ReadOrderOrThrowAsync(response, cancellationToken);
    }

    public async Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/orders/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await apiErrorReader.ReadErrorMessageAsync(response));
        }
    }

    private static async Task<OrderResponse> ReadOrderOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        => await response.Content.ReadFromJsonAsync<OrderResponse>(cancellationToken)
           ?? throw new InvalidOperationException("A API retornou uma resposta inválida.");
}
