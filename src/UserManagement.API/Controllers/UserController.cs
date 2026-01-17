using Microsoft.AspNetCore.Mvc;
using UserManagement.Shared.Contracts.Services;
using UserManagement.Shared.Models.DTOs;
using UserManagement.Shared.Models.Results;

namespace UserManagement.API.Controllers;

/// <summary>
/// API controller for user management endpoints.
/// Handles HTTP requests related to user operations.
/// This controller has no business logic; it delegates to IUserService.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    /// <summary>
    /// Initializes a new instance of the UserController class.
    /// </summary>
    /// <param name="userService">Service for user operations.</param>
    /// <param name="logger">Logger for controller operations.</param>
    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="request">The registration request containing user details.</param>
    /// <returns>
    /// 201 Created if registration is successful with the new user details.
    /// 400 Bad Request if validation fails.
    /// 409 Conflict if the email already exists.
    /// 500 Internal Server Error if an unexpected error occurs.
    /// </returns>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">Validation failed - check error details.</response>
    /// <response code="409">Email already exists in the system.</response>
    /// <response code="500">Internal server error occurred.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterUserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
    {
        try
        {
            _logger.LogInformation("Registration request received for email: {Email}", request?.Email);

            // Call service layer
            var result = await _userService.RegisterUserAsync(request!);

            // Handle success
            if (result.IsSuccess)
            {
                _logger.LogInformation("User registered successfully for email: {Email}", request?.Email);

                var response = ApiResponse<RegisterUserResponse>.SuccessResponse(
                    result.Value!,
                    "User registered successfully");

                // Return 201 Created with Location header
                return CreatedAtAction(
                    nameof(RegisterUser),
                    new { userId = result.Value.UserId },
                    response);
            }

            // Handle failure - email already exists
            if (result.ErrorCode == "EMAIL_ALREADY_EXISTS")
            {
                _logger.LogWarning("Registration failed: Email already exists: {Email}", request?.Email);

                var response = ApiResponse<RegisterUserResponse>.FailureResponse(
                    "Email already exists",
                    result.Errors);

                return Conflict(response);
            }

            // Handle other business logic failures
            _logger.LogWarning("Registration failed for email {Email}: {Error}", request?.Email, result.ErrorMessage);

            var failureResponse = ApiResponse<RegisterUserResponse>.FailureResponse(
                result.ErrorMessage ?? "Registration failed",
                result.Errors);

            return BadRequest(failureResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user registration for email: {Email}", request?.Email);

            var errorResponse = ApiResponse.FailureResponse(
                "An unexpected error occurred during registration",
                "REGISTRATION_ERROR");

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
