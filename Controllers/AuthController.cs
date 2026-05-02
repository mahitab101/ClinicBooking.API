using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private const string AccessTokenCookie = "access_token";
    private const string RefreshTokenCookie = "refresh_token";

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user.
    /// For Doctors: send as multipart/form-data and include the Certificate file.
    /// For Patients/Admins: JSON is fine.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromForm] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);

            // Doctor pending approval — no cookie, just return the info message
            if (result.UserInfo.IsPendingApproval)
                return Ok(result.UserInfo);

            SetTokenCookies(result);
            return Ok(result.UserInfo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            SetTokenCookies(result);
            return Ok(result.UserInfo);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>Silently refresh the access token using the refresh_token cookie</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "No refresh token found." });

        try
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            SetTokenCookies(result);
            return Ok(result.UserInfo);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>Logout — clears both token cookies and revokes the refresh token</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrEmpty(refreshToken))
            await _authService.RevokeRefreshTokenAsync(refreshToken);

        ClearTokenCookies();
        return NoContent();
    }

    /// <summary>Get current logged-in user info from the JWT cookie</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var fullName = User.FindFirst("fullName")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        return Ok(new { userId, email, fullName, roles });
    }

    // ─── Cookie Helpers ───────────────────────────────────────────────────────
    private void SetTokenCookies(TokenResult result)
    {
        var isProduction = HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>().IsProduction();

        Response.Cookies.Append(AccessTokenCookie, result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            Expires = result.AccessTokenExpiresAt
        });

        Response.Cookies.Append(RefreshTokenCookie, result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });
    }

    private void ClearTokenCookies()
    {
        Response.Cookies.Delete(AccessTokenCookie);
        Response.Cookies.Delete(RefreshTokenCookie);
    }
}
