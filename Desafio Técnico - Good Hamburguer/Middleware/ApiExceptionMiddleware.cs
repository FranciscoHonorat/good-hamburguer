using Desafio_Técnico___Good_Hamburguer.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Desafio_Técnico___Good_Hamburguer.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await HandleApiExceptionAsync(context, ex);
        }
    }

    private async Task HandleApiExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception is AppException appException
            ? appException.StatusCode
            : StatusCodes.Status500InternalServerError;

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Erro não tratado na API.");
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Title = statusCode switch
            {
                StatusCodes.Status400BadRequest => "Requisição inválida.",
                StatusCodes.Status401Unauthorized => "Não autorizado.",
                StatusCodes.Status404NotFound => "Recurso não encontrado.",
                _ => "Erro interno no servidor."
            },
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro interno ao processar a requisição."
                : exception.Message,
            Status = statusCode,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
