using Microsoft.AspNetCore.Http;

namespace ClinicBooking.API.Dtos.Auth;

public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Role { get; set; } = "Patient"; // Admin | Doctor | Patient

    // Required only when Role = "Doctor"
    public string? Phone { get; set; }
    public IFormFile? Certificate { get; set; }
}
