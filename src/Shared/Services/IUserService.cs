using Shared.Dto;

namespace Shared.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(string userId);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<IReadOnlyList<UserDto>> GetAllAsync();
    Task<UserDto> CreateAsync(
        string email,
        string displayName,
        string password,
        string storeCode,
        string storeName,
        IEnumerable<string>? roles = null,
        bool mustChangePassword = false);
    Task<UserDto?> UpdateAsync(
        string userId,
        string email,
        string displayName,
        string storeCode,
        string storeName,
        IEnumerable<string>? roles = null);
    Task<UserDto?> ValidateCredentialsAsync(string email, string password);
    Task<UserDto> GetOrCreateEntraUserAsync(string entraObjectId, string email, string displayName);
    Task<UserDto?> ChangePasswordAsync(string userId, string newPassword);
    Task<PasswordResetResultDto?> ResetPasswordAsync(string userId);
    Task<string?> ValidatePasswordPolicyAsync(string password);
    Task DeleteAsync(string userId);
}
