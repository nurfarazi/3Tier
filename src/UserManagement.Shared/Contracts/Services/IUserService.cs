using UserManagement.Shared.Models.DTOs;
using UserManagement.Shared.Models.Results;

namespace UserManagement.Shared.Contracts.Services;

/// <summary>
/// Service interface for user management business logic.
/// Defines the contract for all user-related operations.
/// Implementations handle business logic, validation, and coordination with repositories.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new user in the system.
    /// Performs business logic including email uniqueness validation and password hashing.
    /// </summary>
    /// <param name="request">The registration request containing user data.</param>
    /// <returns>
    /// A Result containing RegisterUserResponse if successful,
    /// or failure details if email already exists or other business rules are violated.
    /// </returns>
    /// <remarks>
    /// This method is idempotent in terms of error handling - calling it multiple times
    /// with the same email will return a failure result each time rather than creating duplicates.
    /// </remarks>
    Task<Result<RegisterUserResponse>> RegisterUserAsync(RegisterUserRequest request);
}
