using Shared.Dto;
using Shared.Models;
using Shared.Repository;
using Shared.Util;

namespace Shared.Services;

/// <summary>
/// <see cref="IUserService"/> の実装クラス。ユーザー管理のビジネスロジックを提供する。
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly UserManagementSettings _settings;

    /// <summary>
    /// <see cref="UserService"/> の新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="repository">ユーザーリポジトリ。</param>
    /// <param name="settings">ユーザー管理設定。<c>null</c> の場合は既定値を使用する。</param>
    public UserService(IUserRepository repository, UserManagementSettings? settings = null)
    {
        _repository = repository;
        _settings = settings ?? new UserManagementSettings();
    }

    /// <inheritdoc/>
    public async Task<UserDto?> GetByIdAsync(string userId)
    {
        var entity = await _repository.GetByIdAsync(userId);
        return entity is null ? null : ToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var entity = await _repository.GetByEmailAsync(email);
        return entity is null ? null : ToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<UserDto> CreateAsync(
        string email,
        string displayName,
        string password,
        string storeCode,
        string storeName,
        IEnumerable<string>? roles = null,
        bool mustChangePassword = false)
    {
        var userId = Guid.NewGuid().ToString();
        var roleList = RoleHelper.NormalizeRoles(roles);

        var entity = new UserEntity
        {
            RowKey = userId,
            Email = email,
            DisplayName = displayName,
            StoreCode = storeCode,
            StoreName = storeName,
            PasswordHash = PasswordHelper.Hash(password),
            RolesJson = JsonHelper.SerializeRoles(roleList),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            MustChangePassword = mustChangePassword,
        };

        await _repository.UpsertAsync(entity);
        return ToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> UpdateAsync(
        string userId,
        string email,
        string displayName,
        string storeCode,
        string storeName,
        IEnumerable<string>? roles = null)
    {
        var entity = await _repository.GetByIdAsync(userId);
        if (entity is null)
        {
            return null;
        }

        entity.Email = email;
        entity.DisplayName = displayName;
        entity.StoreCode = storeCode;
        entity.StoreName = storeName;

        if (roles is not null)
        {
            entity.RolesJson = JsonHelper.SerializeRoles(roles);
        }

        await _repository.UpsertAsync(entity);
        return ToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> ValidateCredentialsAsync(string email, string password)
    {
        var entity = await _repository.GetByEmailAsync(email);
        if (entity is null || !entity.IsActive)
        {
            return null;
        }

        if (!PasswordHelper.Verify(password, entity.PasswordHash))
        {
            return null;
        }

        entity.LastLoginAt = DateTime.UtcNow;
        await _repository.UpsertAsync(entity);

        return ToDto(entity);
    }

    /// <inheritdoc/>
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
            RolesJson = JsonHelper.SerializeRoles([Constants.Roles.General]),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        await _repository.UpsertAsync(entity);
        return ToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<UserDto?> ChangePasswordAsync(string userId, string newPassword)
    {
        var entity = await _repository.GetByIdAsync(userId);
        if (entity is null)
        {
            return null;
        }

        entity.PasswordHash = PasswordHelper.Hash(newPassword);
        entity.MustChangePassword = false;
        await _repository.UpsertAsync(entity);

        return ToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<PasswordResetResultDto?> ResetPasswordAsync(string userId)
    {
        var entity = await _repository.GetByIdAsync(userId);
        if (entity is null)
        {
            return null;
        }

        entity.PasswordHash = PasswordHelper.Hash(_settings.InitialPassword);
        entity.MustChangePassword = true;
        await _repository.UpsertAsync(entity);

        return new PasswordResetResultDto(_settings.InitialPassword, true);
    }

    /// <inheritdoc/>
    public Task<string?> ValidatePasswordPolicyAsync(string password) =>
        Task.FromResult(PasswordPolicyHelper.Validate(password, _settings.PasswordPolicy));

    /// <inheritdoc/>
    public async Task DeleteAsync(string userId) =>
        await _repository.DeleteAsync(userId);

    private static UserDto ToDto(UserEntity entity) =>
        new(
            UserId: entity.RowKey,
            Email: entity.Email,
            DisplayName: entity.DisplayName,
            StoreCode: entity.StoreCode,
            StoreName: entity.StoreName,
            Roles: JsonHelper.DeserializeRoles(entity.RolesJson),
            IsActive: entity.IsActive,
            MustChangePassword: entity.MustChangePassword);
}
