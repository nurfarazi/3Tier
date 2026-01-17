using Serilog;
using FluentValidation.AspNetCore;
using UserManagement.API.Middleware;
using UserManagement.Repository;
using UserManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== Configure Logging (Serilog) ====================
builder.Host.UseSerilog((context, services, config) =>
{
    config
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: Path.Combine(AppContext.BaseDirectory, "logs/usermanagement-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
});

// ==================== Add Services to Container ====================
// Controllers
builder.Services.AddControllers();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validation
builder.Services.AddFluentValidationAutoValidation();
// Validators will be auto-discovered through assembly scanning

// Layer Registration (Dependency Injection)
// Order: Repository -> Services -> No more dependencies
builder.Services.AddRepositoryLayer(builder.Configuration);
builder.Services.AddServiceLayer();

// ==================== Build Application ====================
var app = builder.Build();

// ==================== Configure HTTP Request Pipeline ====================
// Exception handling middleware (must be early in the pipeline)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Generate OpenAPI/Swagger documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Security
app.UseHttpsRedirection();

// Authorization (if needed in future)
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Log application startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting - Environment: {Environment}", app.Environment.EnvironmentName);

// Run application
app.Run();

logger.LogInformation("Application shutting down");
