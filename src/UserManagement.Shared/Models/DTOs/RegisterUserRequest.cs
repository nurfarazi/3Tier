namespace UserManagement.Shared.Models.DTOs;

/// <summary>
/// DTO for user registration requests from the API.
/// Contains all required data for creating a new user account.
/// </summary>
public class RegisterUserRequest
{
    /// <summary>
    /// User's email address. Required and must be unique.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password in plain text. Will be hashed before storage.
    /// Must meet complexity requirements (min 8 chars, upper, lower, digit, special char).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// User's first name. Required.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name. Required.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Optional phone number for contact purposes.
    /// </summary>
    public string? PhoneNumber { get; set; }
}
