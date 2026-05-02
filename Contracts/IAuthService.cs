using ClinicBooking.API.Dtos.Auth;

namespace ClinicBooking.API.Contracts;

public interface IAuthService
{
    Task<TokenResult> RegisterAsync(RegisterDto dto);
    Task<TokenResult> LoginAsync(LoginDto dto);
    Task<TokenResult> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
