using SigepApplication.DTOs.Auth;

namespace SigepApplication.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string username, string password);
    Task<LoginResponseDto?> GetCurrentUserAsync(int userId);
}
