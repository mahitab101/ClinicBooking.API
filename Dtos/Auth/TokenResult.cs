namespace ClinicBooking.API.Dtos.Auth;

/// <summary>
/// Internal result returned by AuthService.
/// Tokens are passed to the controller which sets them as HttpOnly cookies.
/// They are NEVER serialized to the API response body.
/// </summary>
public class TokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public AuthResponseDto UserInfo { get; set; } = null!;
}
