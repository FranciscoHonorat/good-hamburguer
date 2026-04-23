using Desafio_Técnico___Good_Hamburguer.Contracts;

namespace Desafio_Técnico___Good_Hamburguer.Frontend.Services;

public sealed class OrdersApiClient(HttpClient httpClient, ApiErrorReader apiErrorReader)
    : ApiClientBase(httpClient, apiErrorReader)
{
    public Task<PaginatedResponse<OrderResponse>> GetOrdersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => GetOrDefaultAsync(
            $"api/orders?page={page}&pageSize={pageSize}",
            new PaginatedResponse<OrderResponse>
            {
                Items = [],
                Page = page,
                PageSize = pageSize,
                TotalItems = 0,
                TotalPages = 0
            },
            cancellationToken);

    public Task<OrderResponse> CreateOrderAsync(UpsertOrderRequest request, CancellationToken cancellationToken = default)
        => PostRequiredAsync<UpsertOrderRequest, OrderResponse>("api/orders", request, cancellationToken);

    public Task<OrderResponse> UpdateOrderAsync(Guid id, UpsertOrderRequest request, CancellationToken cancellationToken = default)
        => PutRequiredAsync<UpsertOrderRequest, OrderResponse>($"api/orders/{id}", request, cancellationToken);

    public Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/orders/{id}", cancellationToken);
}
