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
                    new { userId = result.Value!.UserId },
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

            // Handle failure - phone number already exists
            if (result.ErrorCode == "PHONE_ALREADY_EXISTS")
            {
                _logger.LogWarning("Registration failed: Phone number already exists: {PhoneNumber}", request?.PhoneNumber);

                var response = ApiResponse<RegisterUserResponse>.FailureResponse(
                    "Phone number already exists",
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

    /// <summary>
    /// Updates an existing user's profile information.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="request">The update request containing new user details.</param>
    /// <returns>
    /// 200 OK if update is successful with the updated user details.
    /// 404 Not Found if the user ID does not exist.
    /// 400 Bad Request if validation fails.
    /// 409 Conflict if business rules like phone uniqueness are violated.
    /// </returns>
    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser([FromRoute] string userId, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (request == null)
                return BadRequest(ApiResponse.FailureResponse("Update request is required"));

            // Ensure the ID in route matches ID in body (or just use route ID)
            request.Id = userId;

            _logger.LogInformation("Update request received for user ID: {UserId}", userId);

            // Call service layer
            var result = await _userService.UpdateUserAsync(request);

            // Handle success
            if (result.IsSuccess)
            {
                _logger.LogInformation("User updated successfully: {UserId}", userId);

                var response = ApiResponse<UpdateUserResponse>.SuccessResponse(
                    result.Value!,
                    "User updated successfully");

                return Ok(response);
            }

            // Handle failure - not found
            if (result.ErrorCode == "USER_NOT_FOUND")
            {
                _logger.LogWarning("Update failed: User with ID {UserId} not found", userId);
                return NotFound(ApiResponse.FailureResponse(result.ErrorMessage ?? "User not found"));
            }

            // Handle failure - uniqueness conflict (e.g. phone)
            if (result.ErrorCode == "PHONE_ALREADY_EXISTS")
            {
                _logger.LogWarning("Update failed: Phone number already exists for user {UserId}", userId);
                return Conflict(ApiResponse.FailureResponse(result.ErrorMessage ?? "Phone number already exists", result.Errors));
            }

            // Handle other business logic failures
            _logger.LogWarning("Update failed for user {UserId}: {Error}", userId, result.ErrorMessage);

            var failureResponse = ApiResponse<UpdateUserResponse>.FailureResponse(
                result.ErrorMessage ?? "Update failed",
                result.Errors);

            return BadRequest(failureResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user update for ID: {UserId}", userId);

            var errorResponse = ApiResponse.FailureResponse(
                "An unexpected error occurred during update",
                "UPDATE_ERROR");

            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
