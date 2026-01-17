using Microsoft.Extensions.Logging;
using UserManagement.Shared.Contracts.Repositories;
using UserManagement.Shared.Contracts.Services;
using UserManagement.Shared.Models.DTOs;
using UserManagement.Shared.Models.Entities;
using UserManagement.Shared.Models.Results;

namespace UserManagement.Services.Implementations;

/// <summary>
/// Service implementation for user management business logic.
/// Handles user registration, validation, and coordination with repository layer.
/// All password hashing is done here before repository access.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Initializes a new instance of the UserService class.
    /// </summary>
    /// <param name="userRepository">Repository for user data access.</param>
    /// <param name="logger">Logger for service operations.</param>
    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new user in the system.
    /// Performs the following steps:
    /// 1. Check if email already exists (business rule: email uniqueness)
    /// 2. Hash the password using BCrypt (security requirement)
    /// 3. Create User entity from DTO
    /// 4. Persist to database via repository
    /// 5. Return success response with user data
    /// </summary>
    /// <param name="request">The registration request containing user data.</param>
    /// <returns>Success result with user details, or failure result if business rules violated.</returns>
    public async Task<Result<RegisterUserResponse>> RegisterUserAsync(RegisterUserRequest request)
    {
        try
        {
            // Validate input
            if (request == null)
                return Result<RegisterUserResponse>.Failure(
                    "Registration request is required",
                    "INVALID_REQUEST");

            if (string.IsNullOrWhiteSpace(request.Email))
                return Result<RegisterUserResponse>.Failure(
                    "Email is required",
                    "MISSING_EMAIL");

            if (string.IsNullOrWhiteSpace(request.Password))
                return Result<RegisterUserResponse>.Failure(
                    "Password is required",
                    "MISSING_PASSWORD");

            _logger.LogInformation("User registration started for email: {Email}", request.Email);

            // Business Rule 1: Check if email already exists
            _logger.LogInformation("Checking if email already exists: {Email}", request.Email);
            var emailExists = await _userRepository.EmailExistsAsync(request.Email);

            if (emailExists)
            {
                _logger.LogWarning("Registration failed: Email already exists: {Email}", request.Email);
                return Result<RegisterUserResponse>.Failure(
                    "Email already exists",
                    new List<string> { "A user with this email address is already registered" },
                    "EMAIL_ALREADY_EXISTS");
            }

            // Hash password using BCrypt
            _logger.LogInformation("Hashing password for user: {Email}", request.Email);
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

            // Create User domain entity
            _logger.LogInformation("Creating user entity for email: {Email}", request.Email);
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Persist to database
            _logger.LogInformation("Persisting user to database: {Email}", request.Email);
            var createdUser = await _userRepository.AddAsync(user);

            // Map to response DTO
            _logger.LogInformation("User registration completed successfully for email: {Email}", request.Email);
            var response = new RegisterUserResponse
            {
                UserId = createdUser.Id,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                CreatedAt = createdUser.CreatedAt
            };

            return Result<RegisterUserResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user registration for email: {Email}", request?.Email);
            return Result<RegisterUserResponse>.Failure(
                ex,
                "REGISTRATION_ERROR");
        }
    }
}
