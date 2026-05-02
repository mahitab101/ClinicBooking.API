namespace ClinicBooking.API.Dtos.Auth;

public class AuthResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime AccessTokenExpiresAt { get; set; }

    /// <summary>True when a Doctor registers — no token is issued yet</summary>
    public bool IsPendingApproval { get; set; } = false;
}
