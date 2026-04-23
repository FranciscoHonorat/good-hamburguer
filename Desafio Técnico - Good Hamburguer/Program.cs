using Desafio_Técnico___Good_Hamburguer.Components;
using Desafio_Técnico___Good_Hamburguer.Configuration;
using Desafio_Técnico___Good_Hamburguer.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();

var app = builder.Build();
app.ApplyDatabaseMigrations();
app.ConfigureApplicationPipeline();
app.MapApiEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;
