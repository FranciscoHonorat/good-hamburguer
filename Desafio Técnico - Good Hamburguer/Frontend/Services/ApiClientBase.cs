using System.Net.Http.Json;

namespace Desafio_Técnico___Good_Hamburguer.Frontend.Services;

public abstract class ApiClientBase(HttpClient httpClient, ApiErrorReader apiErrorReader)
{
    protected async Task<TResponse> GetRequiredAsync<TResponse>(string url, CancellationToken cancellationToken = default)
        where TResponse : class
    {
        using var response = await httpClient.GetAsync(url, cancellationToken);
        return await ReadRequiredPayloadAsync<TResponse>(response, cancellationToken);
    }

    protected async Task<TResponse> PostRequiredAsync<TRequest, TResponse>(string url, TRequest payload, CancellationToken cancellationToken = default)
        where TResponse : class
    {
        using var response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        return await ReadRequiredPayloadAsync<TResponse>(response, cancellationToken);
    }

    protected async Task<TResponse> PutRequiredAsync<TRequest, TResponse>(string url, TRequest payload, CancellationToken cancellationToken = default)
        where TResponse : class
    {
        using var response = await httpClient.PutAsJsonAsync(url, payload, cancellationToken);
        return await ReadRequiredPayloadAsync<TResponse>(response, cancellationToken);
    }

    protected async Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(url, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    protected async Task<TResponse?> GetOptionalAsync<TResponse>(string url, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(url, cancellationToken);
        return await ReadOptionalPayloadAsync<TResponse>(response, cancellationToken);
    }

    protected async Task<TResponse?> ReadOptionalPayloadAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    protected async Task<TResponse> GetOrDefaultAsync<TResponse>(string url, TResponse defaultValue, CancellationToken cancellationToken = default)
    {
        var payload = await GetOptionalAsync<TResponse>(url, cancellationToken);
        return payload is null ? defaultValue : payload;
    }

    private async Task<TResponse> ReadRequiredPayloadAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
        where TResponse : class
        => await ReadOptionalPayloadAsync<TResponse>(response, cancellationToken)
           ?? throw new InvalidOperationException("A API retornou uma resposta inválida.");

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await apiErrorReader.ReadErrorMessageAsync(response));
        }
    }
}
