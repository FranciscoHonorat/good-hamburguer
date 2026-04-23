using Desafio_Técnico___Good_Hamburguer.Data;
using Desafio_Técnico___Good_Hamburguer.Middleware;
using Microsoft.EntityFrameworkCore;

namespace Desafio_Técnico___Good_Hamburguer.Configuration;

public static class WebApplicationExtensions
{
    private const long MaxApiPayloadSizeBytes = 32 * 1024;

    public static WebApplication ApplyDatabaseMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (dbContext.Database.IsRelational())
        {
            dbContext.Database.Migrate();
        }

        return app;
    }

    public static WebApplication ConfigureApplicationPipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(ServiceCollectionExtensions.CorsPolicyName);
        app.Use(ValidateApiPayloadSizeAsync);
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<ApiExceptionMiddleware>();
        app.UseMiddleware<ApiKeyMiddleware>();
        app.UseRateLimiter();
        app.UseAntiforgery();

        return app;
    }

    private static async Task ValidateApiPayloadSizeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) &&
            context.Request.ContentLength is > MaxApiPayloadSizeBytes)
        {
            await ProblemDetailsResponseWriter.WriteAsync(
                context,
                StatusCodes.Status413PayloadTooLarge,
                $"O tamanho máximo permitido para requisições da API é de {MaxApiPayloadSizeBytes} bytes.");

            return;
        }

        await next(context);
    }
}
