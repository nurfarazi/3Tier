using Microsoft.Extensions.DependencyInjection;
using UserManagement.Services.Implementations;
using UserManagement.Services.Validators;
using UserManagement.Shared.Configuration;
using UserManagement.Shared.Contracts.Services;
using UserManagement.Shared.Contracts.Validators;
using UserManagement.Shared.Models.Entities;

namespace UserManagement.Services;

/// <summary>
/// Dependency injection extension methods for the Service layer.
/// Registers all service implementations.
/// </summary>
public static class ServiceDependencyInjection
{
    /// <summary>
    /// Adds all service layer implementations to the DI container.
    /// Services are registered as scoped per HTTP request.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        // Register all services as scoped
        // Scoped means a new instance per HTTP request in web context
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Register Business Validators
        services.AddScoped<IBusinessValidator<User>, EmailUniquenessValidator>();
        services.AddScoped<IBusinessValidator<User>, PhoneUniquenessValidator>();

        return services;
    }
}
