using UserManagement.Shared.Models.Entities;

namespace UserManagement.Shared.Contracts.Repositories;

/// <summary>
/// Repository interface for User entities.
/// Extends the generic IRepository with user-specific data access methods.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Retrieves a user by their email address.
    /// Email is a unique identifier in the system and commonly used for lookups.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The user if found; null otherwise.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Checks if a user with the given email already exists.
    /// Used for validation during registration to ensure email uniqueness.
    /// </summary>
    /// <param name="email">The email address to check for existence.</param>
    /// <returns>True if a user with this email exists; false otherwise.</returns>
    Task<bool> EmailExistsAsync(string email);
}
