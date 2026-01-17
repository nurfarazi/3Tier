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
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character");
    }

    /// <summary>
    /// Configures first and last name validation rules.
    /// </summary>
    private void ConfigureNameValidation()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(50)
            .WithMessage("First name must not exceed 50 characters")
            .Matches("^[a-zA-Z ]+$")
            .WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name must not exceed 50 characters")
            .Matches("^[a-zA-Z ]+$")
            .WithMessage("Last name can only contain letters and spaces");
    }

    /// <summary>
    /// Configures phone number validation rules.
    /// Phone number is optional but must match a valid format if provided.
    /// </summary>
    private void ConfigurePhoneValidation()
    {
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?1?\d{9,15}$")
            .WithMessage("Phone number must be a valid phone format")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
