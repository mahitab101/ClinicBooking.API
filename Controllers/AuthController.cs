using ClinicBooking.API.Common;
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

    [HttpPost("register")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Register([FromForm] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (result.UserInfo.IsPendingApproval)
            return Ok(ApiResponse<AuthResponseDto>.Ok(result.UserInfo,
                "Registration successful. Your account is pending admin approval."));

        SetTokenCookies(result);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result.UserInfo, "Registration successful."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        SetTokenCookies(result);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result.UserInfo, "Login successful."));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(ApiResponse.FailNoData("No refresh token found."));

        var result = await _authService.RefreshTokenAsync(refreshToken);
        SetTokenCookies(result);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result.UserInfo, "Token refreshed."));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        if (!string.IsNullOrEmpty(refreshToken))
            await _authService.RevokeRefreshTokenAsync(refreshToken);

        ClearTokenCookies();
        return Ok(ApiResponse.OkNoData("Logged out successfully."));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var fullName = User.FindFirst("fullName")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(ApiResponse<object>.Ok(new { userId, email, fullName, roles }));
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
