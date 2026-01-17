using FluentValidation;
using UserManagement.Shared.Models.DTOs;

namespace UserManagement.API.Validators;

/// <summary>
/// FluentValidation validator for RegisterUserRequest.
/// Defines all data-level validation rules for user registration.
/// This validator is automatically applied to RegisterUserRequest DTOs via ASP.NET Core integration.
/// </summary>
public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    /// <summary>
    /// Initializes a new instance of the RegisterUserRequestValidator class.
    /// Configures all validation rules for user registration.
    /// </summary>
    public RegisterUserRequestValidator()
    {
        ConfigureEmailValidation();
        ConfigurePasswordValidation();
        ConfigureNameValidation();
        ConfigurePhoneValidation();
        ConfigureDateOfBirthValidation();
    }

    /// <summary>
    /// Configures email validation rules.
    /// </summary>
    private void ConfigureEmailValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .MaximumLength(100)
            .WithMessage("Email must not exceed 100 characters")
            .EmailAddress()
            .WithMessage("Email must be a valid email address");
    }

    /// <summary>
    /// Configures password validation rules.
    /// Password must meet complexity requirements for security.
    /// </summary>
    private void ConfigurePasswordValidation()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(10)
            .WithMessage("Password must be at least 10 characters long")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character")
            .Must((request, password) => !password.Contains(request.Email.Split('@')[0]))
            .WithMessage("Password must not contain your email prefix")
            .Must((request, password) => string.IsNullOrWhiteSpace(request.PhoneNumber) || !password.Contains(request.PhoneNumber.Length > 6 ? request.PhoneNumber[^6..] : request.PhoneNumber))
            .WithMessage("Password must not contain a substring of your phone number")
            .Must(password => !GetWeakPasswords().Contains(password.ToLower()))
            .WithMessage("Password is too common and easily guessable");
    }

    /// <summary>
    /// Configures first and last name validation rules.
    /// </summary>
    private void ConfigureNameValidation()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .Length(2, 50)
            .WithMessage("First name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$")
            .WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .Length(2, 50)
            .WithMessage("Last name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$")
            .WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");
    }

    /// <summary>
    /// Configures phone number validation rules.
    /// </summary>
    private void ConfigurePhoneValidation()
    {
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{7,15}$")
            .WithMessage("Phone number must be between 7 and 15 digits and can optionally start with '+'")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }

    /// <summary>
    /// Configures date of birth validation rules.
    /// </summary>
    private void ConfigureDateOfBirthValidation()
    {
        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob == null || dob.Value <= DateTime.Today.AddYears(-13))
            .WithMessage("You must be at least 13 years old to register")
            .When(x => x.DateOfBirth.HasValue);
    }

    /// <summary>
    /// A small list of common weak passwords for validation.
    /// In a production environment, this should be a larger list or a more robust service.
    /// </summary>
    private static IEnumerable<string> GetWeakPasswords()
    {
        return new[] { "password", "password123", "1234567890", "qwertyuiop", "admin123", "welcome123" };
    }
}
