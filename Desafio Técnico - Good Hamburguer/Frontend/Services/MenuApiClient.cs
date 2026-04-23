using Desafio_Técnico___Good_Hamburguer.Contracts;

namespace Desafio_Técnico___Good_Hamburguer.Frontend.Services;

public sealed class MenuApiClient(HttpClient httpClient, ApiErrorReader apiErrorReader)
    : ApiClientBase(httpClient, apiErrorReader)
{
    public Task<IReadOnlyList<MenuItemResponse>> GetMenuAsync(CancellationToken cancellationToken = default)
        => GetOrDefaultAsync<IReadOnlyList<MenuItemResponse>>("api/menu", [], cancellationToken);
}
