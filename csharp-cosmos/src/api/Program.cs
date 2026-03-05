using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Todo.Core.Infrastructure;
using Todo.Core.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Serilog as the logging provider
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

// Core infrastructure (Cosmos and shared registrations via extensions)
builder.Services.AddCoreInfrastructure(builder.Configuration);

builder.Services.AddCors();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();
builder.Services.AddControllers(options => options.Filters.Add<Todo.Core.Middleware.ValidationFilter>());

var app = builder.Build();

// Middleware order: logging → CORS → error handling → validation → routing
app.UseRequestLogging();
app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});
app.UseErrorHandling();
app.UseValidation();

// Swagger UI
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("./openapi.yaml", "v1");
    options.RoutePrefix = "";
});

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
});

app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = async (context, report) => await context.Response.WriteAsync("Healthy") });
app.MapGet("/", () => Results.Ok("OK"));

if (app.Environment.IsDevelopment() || !string.IsNullOrEmpty(app.Configuration["INITIALIZE_COSMOS_ON_STARTUP"]))
{
    try
    {
        await app.EnsureDatabaseCreatedAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database initialization failed.");
        throw;
    }
}

app.Run();
