using Shared.Dto;

namespace Shared.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(string userId);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<IReadOnlyList<UserDto>> GetAllAsync();
    Task<UserDto> CreateAsync(string email, string displayName, string password, IEnumerable<string>? roles = null);
    Task<UserDto?> ValidateCredentialsAsync(string email, string password);
    Task<UserDto> GetOrCreateEntraUserAsync(string entraObjectId, string email, string displayName);
    Task DeleteAsync(string userId);
}
