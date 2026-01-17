using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using UserManagement.Shared.Contracts.Repositories;
using UserManagement.Shared.Models.Entities;

namespace UserManagement.Repository.Implementations;

/// <summary>
/// MongoDB repository implementation for User entities.
/// Provides user-specific data access methods while inheriting generic CRUD operations from BaseRepository.
/// Handles MongoDB-specific operations for user management.
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the UserRepository class.
    /// Creates indexes on application startup to optimize queries.
    /// </summary>
    /// <param name="database">The MongoDB database instance.</param>
    /// <param name="logger">Logger for repository operations.</param>
    public UserRepository(IMongoDatabase database, ILogger<UserRepository> logger)
        : base(database, logger)
    {
        // Create indexes on startup
        EnsureIndexesAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// Email uniqueness is a business requirement, so this is a common lookup.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The user if found; null otherwise.</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            Logger.LogInformation("Searching for user with email: {Email}", email);

            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var user = await Collection.Find(filter).FirstOrDefaultAsync();

            if (user != null)
                Logger.LogInformation("User found with email: {Email}", email);
            else
                Logger.LogInformation("User not found with email: {Email}", email);

            return user;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Checks if a user with the given email already exists.
    /// Used during registration validation to ensure email uniqueness.
    /// This check is case-insensitive.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>True if a user with this email exists; false otherwise.</returns>
    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            Logger.LogInformation("Checking if email exists (normalized): {Email}", normalizedEmail);

            var filter = Builders<User>.Filter.Eq(u => u.Email, normalizedEmail);
            var count = await Collection.CountDocumentsAsync(filter);

            var exists = count > 0;
            Logger.LogInformation("Email existence check result for {Email}: {Exists}", normalizedEmail, exists);

            return exists;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking email existence: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Checks if a user with the given phone number already exists.
    /// Used during registration validation to ensure phone number uniqueness.
    /// </summary>
    /// <param name="phoneNumber">The phone number to check.</param>
    /// <returns>True if a user with this phone number exists; false otherwise.</returns>
    public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
    {
        try
        {
            Logger.LogInformation("Checking if phone number exists: {PhoneNumber}", phoneNumber);

            var filter = Builders<User>.Filter.Eq(u => u.PhoneNumber, phoneNumber);
            var count = await Collection.CountDocumentsAsync(filter);

            var exists = count > 0;
            Logger.LogInformation("Phone number existence check result for {PhoneNumber}: {Exists}", phoneNumber, exists);

            return exists;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking phone number existence: {PhoneNumber}", phoneNumber);
            throw;
        }
    }

    /// <summary>
    /// Ensures that MongoDB indexes are created for optimal query performance.
    /// Should be called during application startup.
    /// </summary>
    private async Task EnsureIndexesAsync()
    {
        try
        {
            Logger.LogInformation("Creating indexes for User collection");

            // Create unique index on Email field
            var emailIndexModel = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }
            );

            // Create unique index on PhoneNumber field (sparse since it's optional)
            var phoneIndexModel = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.PhoneNumber),
                new CreateIndexOptions { Unique = true, Sparse = true }
            );

            await Collection.Indexes.CreateManyAsync(new[] { emailIndexModel, phoneIndexModel });

            Logger.LogInformation("Indexes created successfully for User collection");
        }
        catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict" || ex.CodeName == "DuplicateKey")
        {
            // Index already exists, this is fine
            Logger.LogInformation("Index already exists for User collection");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating indexes for User collection");
            throw;
        }
    }
}
