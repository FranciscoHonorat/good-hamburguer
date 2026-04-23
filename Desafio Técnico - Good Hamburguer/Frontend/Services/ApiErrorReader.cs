using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Desafio_Técnico___Good_Hamburguer.Frontend.Services;

public sealed class ApiErrorReader
{
    public async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        if (response.Content is null)
        {
            return BuildDefaultErrorMessage(response);
        }

        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (!string.IsNullOrWhiteSpace(problem?.Detail))
            {
                return problem.Detail;
            }
        }
        catch (Exception)
        {
        }

        return BuildDefaultErrorMessage(response);
    }

    private static string BuildDefaultErrorMessage(HttpResponseMessage response)
        => $"Erro na API ({(int)response.StatusCode} - {response.ReasonPhrase ?? "sem detalhe"}).";
}
