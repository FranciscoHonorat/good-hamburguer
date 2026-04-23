using Microsoft.AspNetCore.Mvc;

namespace Desafio_Técnico___Good_Hamburguer.Middleware;

internal static class ProblemDetailsResponseWriter
{
    public static Task WriteAsync(HttpContext context, int statusCode, string detail, string? title = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Title = title ?? ResolveTitle(statusCode),
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        return context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static string ResolveTitle(int statusCode)
        => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Requisição inválida.",
            StatusCodes.Status401Unauthorized => "Não autorizado.",
            StatusCodes.Status404NotFound => "Recurso não encontrado.",
            StatusCodes.Status413PayloadTooLarge => "Payload muito grande.",
            _ => "Erro interno no servidor."
        };
}
