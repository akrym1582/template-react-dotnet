using Shared.Dto;
using Shared.Models;
using Shared.Repository;
using Shared.Util;

namespace Shared.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserDto?> GetByIdAsync(string userId)
    {
        var entity = await _repository.GetByIdAsync(userId);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var entity = await _repository.GetByEmailAsync(email);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(ToDto).ToList();
    }

    public async Task<UserDto> CreateAsync(
        string email, string displayName, string password, IEnumerable<string>? roles = null)
    {
        var userId = Guid.NewGuid().ToString();
        var roleList = roles?.ToList() ?? [Constants.Roles.User];

        var entity = new UserEntity
        {
            RowKey = userId,
            Email = email,
            DisplayName = displayName,
            PasswordHash = PasswordHelper.Hash(password),
            RolesJson = JsonHelper.SerializeRoles(roleList),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.UpsertAsync(entity);
        return ToDto(entity);
    }

    public async Task<UserDto?> ValidateCredentialsAsync(string email, string password)
    {
        var entity = await _repository.GetByEmailAsync(email);
        if (entity is null || !entity.IsActive)
            return null;

        if (!PasswordHelper.Verify(password, entity.PasswordHash))
            return null;

        entity.LastLoginAt = DateTime.UtcNow;
        await _repository.UpsertAsync(entity);

        return ToDto(entity);
    }

    public async Task<UserDto> GetOrCreateEntraUserAsync(
        string entraObjectId, string email, string displayName)
    {
        var entity = await _repository.GetByEntraObjectIdAsync(entraObjectId);

        if (entity is not null)
        {
            entity.LastLoginAt = DateTime.UtcNow;
            await _repository.UpsertAsync(entity);
            return ToDto(entity);
        }

        var userId = Guid.NewGuid().ToString();
        entity = new UserEntity
        {
            RowKey = userId,
            Email = email,
            DisplayName = displayName,
            EntraObjectId = entraObjectId,
            RolesJson = JsonHelper.SerializeRoles([Constants.Roles.User]),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.UpsertAsync(entity);
        return ToDto(entity);
    }

    public async Task DeleteAsync(string userId) =>
        await _repository.DeleteAsync(userId);

    private static UserDto ToDto(UserEntity entity) =>
        new(
            UserId: entity.RowKey,
            Email: entity.Email,
            DisplayName: entity.DisplayName,
            Roles: JsonHelper.DeserializeRoles(entity.RolesJson),
            IsActive: entity.IsActive);
}
