using UserDashboard.API.DTOs;
using UserDashboard.API.Models;

namespace UserDashboard.API.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterDto dto);
    Task<string> LoginAsync(LoginDto dto);
    Task<User?> GetUserByEmailAsync(string email);
}
