using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Dtos.Auth;
using ClinicBooking.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ClinicBooking.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _env;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _env = env;
    }

    // ─── Register ────────────────────────────────────────────────────────────
    public async Task<TokenResult> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        // Validate doctor-specific requirements
        if (dto.Role == "Doctor" && dto.Certificate is null)
            throw new InvalidOperationException("Doctors must upload a certificate.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = $"{dto.FirstName} {dto.LastName}",
            Email = dto.Email,
            UserName = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        var validRoles = new[] { "Admin", "Doctor", "Patient" };
        var role = validRoles.Contains(dto.Role) ? dto.Role : "Patient";
        await _userManager.AddToRoleAsync(user, role);

        // ── Create linked profile ──────────────────────────────────────────
        if (role == "Patient")
        {
            var patient = new Patient
            {
                FullName = user.FullName,
                Email = user.Email!,
                Phone = dto.Phone ?? string.Empty,
                UserId = user.Id
            };
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();
        }
        else if (role == "Doctor")
        {
            var certificateUrl = await SaveCertificateAsync(dto);

            var doctor = new Doctor
            {
                FullName = user.FullName,
                Email = user.Email!,
                Phone = dto.Phone ?? string.Empty,
                UserId = user.Id,
                CertificateUrl = certificateUrl,
                IsApproved = false,
                IsActive = false
            };
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            // Notify admin about pending approval
            await _emailService.SendDoctorPendingApprovalToAdminAsync(user.FullName, user.Email!);

            // Return without issuing tokens — doctor must wait for approval
            return new TokenResult
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                AccessTokenExpiresAt = DateTime.UtcNow,
                UserInfo = new AuthResponseDto
                {
                    Email = user.Email!,
                    FullName = user.FullName,
                    Roles = new List<string> { "Doctor" },
                    AccessTokenExpiresAt = DateTime.UtcNow,
                    IsPendingApproval = true
                }
            };
        }

        return await BuildTokenResultAsync(user);
    }

    // ─── Login ───────────────────────────────────────────────────────────────
    public async Task<TokenResult> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Block unapproved doctors
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Doctor"))
        {
            var doctor = _unitOfWork.Doctors.Query()
                .FirstOrDefault(d => d.UserId == user.Id);

            if (doctor is null || !doctor.IsApproved)
                throw new UnauthorizedAccessException(
                    "Your account is pending admin approval. You will receive an email once approved.");

            if (doctor.IsRejected)
                throw new UnauthorizedAccessException(
                    $"Your account was rejected. Reason: {doctor.RejectionReason}");
        }

        return await BuildTokenResultAsync(user);
    }

    // ─── Refresh Token ───────────────────────────────────────────────────────
    public async Task<TokenResult> RefreshTokenAsync(string refreshToken)
    {
        var user = _userManager.Users
            .FirstOrDefault(u => u.RefreshToken == refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired. Please log in again.");

        return await BuildTokenResultAsync(user);
    }

    // ─── Revoke ──────────────────────────────────────────────────────────────
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var user = _userManager.Users
            .FirstOrDefault(u => u.RefreshToken == refreshToken);

        if (user is null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private async Task<TokenResult> BuildTokenResultAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var jwtSettings = _configuration.GetSection("JWT");
        var durationMinutes = int.Parse(jwtSettings["DurationInMinutes"] ?? "60");

        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(durationMinutes);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new TokenResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = expiresAt,
            UserInfo = new AuthResponseDto
            {
                Email = user.Email!,
                FullName = user.FullName,
                Roles = roles,
                AccessTokenExpiresAt = expiresAt,
                IsPendingApproval = false
            }
        };
    }

    private async Task<string> SaveCertificateAsync(RegisterDto dto)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "certificates");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(dto.Certificate!.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await dto.Certificate.CopyToAsync(stream);

        return $"/certificates/{fileName}";
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var durationMinutes = int.Parse(jwtSettings["DurationInMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new("fullName", user.FullName),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(durationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
