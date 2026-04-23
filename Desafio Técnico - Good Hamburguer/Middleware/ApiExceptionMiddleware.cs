using Desafio_Técnico___Good_Hamburguer.Exceptions;

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

        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? "Ocorreu um erro interno ao processar a requisição."
            : exception.Message;

        await ProblemDetailsResponseWriter.WriteAsync(context, statusCode, detail);
    }
}
