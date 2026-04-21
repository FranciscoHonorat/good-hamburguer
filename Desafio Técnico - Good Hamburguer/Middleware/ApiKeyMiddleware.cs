using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Desafio_Técnico___Good_Hamburguer.Middleware;

public sealed class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string HeaderName = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var configuredApiKey = configuration["Security:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var providedApiKey) ||
            !IsApiKeyValid(configuredApiKey, providedApiKey.ToString()))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";
            var problemDetails = new ProblemDetails
            {
                Title = "Não autorizado.",
                Detail = "Chave de API inválida ou ausente.",
                Status = StatusCodes.Status401Unauthorized,
                Instance = context.Request.Path
            };
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        await next(context);
    }

    private static bool IsApiKeyValid(string configuredApiKey, string providedApiKey)
    {
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return false;
        }

        var configuredApiKeyBytes = Encoding.UTF8.GetBytes(configuredApiKey);
        var providedApiKeyBytes = Encoding.UTF8.GetBytes(providedApiKey);

        return configuredApiKeyBytes.Length == providedApiKeyBytes.Length &&
               CryptographicOperations.FixedTimeEquals(configuredApiKeyBytes, providedApiKeyBytes);
    }
}
