using Desafio_Técnico___Good_Hamburguer.Contracts;
using System.Net.Http.Json;

namespace Desafio_Técnico___Good_Hamburguer.Frontend.Services;

public sealed class MenuApiClient(HttpClient httpClient, ApiErrorReader apiErrorReader)
{
    public async Task<IReadOnlyList<MenuItemResponse>> GetMenuAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("api/menu", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await apiErrorReader.ReadErrorMessageAsync(response));
        }

        var menu = await response.Content.ReadFromJsonAsync<IReadOnlyList<MenuItemResponse>>(cancellationToken);
        return menu ?? [];
    }
}
