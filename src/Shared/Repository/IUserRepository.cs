using Shared.Models;

namespace Shared.Repository;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(string userId);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity?> GetByEntraObjectIdAsync(string entraObjectId);
    Task<IReadOnlyList<UserEntity>> GetAllAsync();
    Task UpsertAsync(UserEntity user);
    Task DeleteAsync(string userId);
}
