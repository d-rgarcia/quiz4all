using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using NetTemplateApplication.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

const string IDENTITY_DATABASE_CONNECTION = "IdentityDatabase";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Global exception handler:
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// In order for ClientInfo serilog enricher:
builder.Services.AddHttpContextAccessor();

IHostEnvironment env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

builder.Services.Configure<CorrelationIdOptions>(
    builder.Configuration.GetSection(nameof(CorrelationIdOptions))
);

builder.Services.AddAuthorizationBuilder();

builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(IDENTITY_DATABASE_CONNECTION))
);

builder.Services.AddIdentityApiEndpoints<User>()
                .AddEntityFrameworkStores<IdentityContext>();

builder.Services.AddHealthChecks()
#pragma warning disable CS8604 // Possible null reference argument.
    .AddNpgSql(builder.Configuration.GetConnectionString(IDENTITY_DATABASE_CONNECTION))
#pragma warning restore CS8604 // Possible null reference argument.
    .AddCheck<SeqHealthCheck>("seq-service", Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);

builder.Services.AddHttpClient<SeqHealthCheck>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetSection("SeqHealthCheckBaseAddress").Get<string>() ??
        throw new ArgumentException("SeqHealthCheckBaseAddress not configured"));
});

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("NetTemplate"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddRedisInstrumentation()
            .AddNpgsql();

        tracing.AddOtlpExporter();
    });

// Values could be read from options. But for Health checks endpoint it is okay...
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 2;
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
});

var app = builder.Build();

// Global exception handler:
app.UseExceptionHandler();

// Custom middlewares:
app.UseCorrelationIdMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter();
// Group identity endpoints, host/identity/login for example:
app.MapGroup("/identity").MapIdentityApi<User>();

app.MapHealthChecks("/health")
    .RequireRateLimiting("fixed");

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
