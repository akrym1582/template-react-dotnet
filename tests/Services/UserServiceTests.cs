using NSubstitute;
using Shared.Dto;
using Shared.Models;
using Shared.Repository;
using Shared.Services;
using Shared.Util;

namespace Tests.Services;

public class UserServiceTests
{
    private readonly IUserRepository _repository;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repository = Substitute.For<IUserRepository>();
        _service = new UserService(
            _repository,
            new UserManagementSettings
            {
                InitialPassword = "Init@1234",
                PasswordPolicy = new PasswordPolicySettings
                {
                    MinLength = 8,
                    RequireUppercase = true,
                    RequireLowercase = true,
                    RequireDigit = true,
                    RequireSpecialCharacter = true,
                },
            });
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsDto()
    {
        var entity = CreateTestEntity("user1", "test@example.com", "Test User");
        _repository.GetByIdAsync("user1").Returns(entity);

        var result = await _service.GetByIdAsync("user1");

        Assert.NotNull(result);
        Assert.Equal("user1", result.UserId);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentUser_ReturnsNull()
    {
        _repository.GetByIdAsync("unknown").Returns((UserEntity?)null);

        var result = await _service.GetByIdAsync("unknown");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesUserWithDefaultRole()
    {
        UserEntity? captured = null;
        _repository.UpsertAsync(Arg.Do<UserEntity>(u => captured = u)).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync("new@example.com", "New User", "Password123!", "001", "本店");

        Assert.NotNull(result);
        Assert.Equal("new@example.com", result.Email);
        Assert.Contains(Constants.Roles.General, result.Roles);
        Assert.Equal("001", result.StoreCode);
        Assert.Equal("本店", result.StoreName);

        Assert.NotNull(captured);
        Assert.True(PasswordHelper.Verify("Password123!", captured.PasswordHash));
    }

    [Fact]
    public async Task ValidateCredentialsAsync_CorrectPassword_ReturnsUser()
    {
        var entity = CreateTestEntity("user1", "test@example.com", "Test User");
        entity.PasswordHash = PasswordHelper.Hash("Password123!");
        _repository.GetByEmailAsync("test@example.com").Returns(entity);
        _repository.UpsertAsync(Arg.Any<UserEntity>()).Returns(Task.CompletedTask);

        var result = await _service.ValidateCredentialsAsync("test@example.com", "Password123!");

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WrongPassword_ReturnsNull()
    {
        var entity = CreateTestEntity("user1", "test@example.com", "Test User");
        entity.PasswordHash = PasswordHelper.Hash("Password123!");
        _repository.GetByEmailAsync("test@example.com").Returns(entity);

        var result = await _service.ValidateCredentialsAsync("test@example.com", "WrongPassword");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrCreateEntraUserAsync_NewUser_CreatesUser()
    {
        _repository.GetByEntraObjectIdAsync("oid-123").Returns((UserEntity?)null);
        _repository.UpsertAsync(Arg.Any<UserEntity>()).Returns(Task.CompletedTask);

        var result = await _service.GetOrCreateEntraUserAsync("oid-123", "entra@example.com", "Entra User");

        Assert.NotNull(result);
        Assert.Equal("entra@example.com", result.Email);
        await _repository.Received(1).UpsertAsync(Arg.Any<UserEntity>());
    }

    [Fact]
    public async Task GetOrCreateEntraUserAsync_ExistingUser_ReturnsExisting()
    {
        var entity = CreateTestEntity("user1", "entra@example.com", "Entra User");
        entity.EntraObjectId = "oid-123";
        _repository.GetByEntraObjectIdAsync("oid-123").Returns(entity);
        _repository.UpsertAsync(Arg.Any<UserEntity>()).Returns(Task.CompletedTask);

        var result = await _service.GetOrCreateEntraUserAsync("oid-123", "entra@example.com", "Entra User");

        Assert.NotNull(result);
        Assert.Equal("user1", result.UserId);
    }

    [Fact]
    public async Task ResetPasswordAsync_初期化時はMustChangePasswordがtrueになる()
    {
        var entity = CreateTestEntity("user1", "test@example.com", "Test User");
        _repository.GetByIdAsync("user1").Returns(entity);
        _repository.UpsertAsync(Arg.Any<UserEntity>()).Returns(Task.CompletedTask);

        var result = await _service.ResetPasswordAsync("user1");

        Assert.NotNull(result);
        Assert.Equal("Init@1234", result.InitialPassword);
        Assert.True(result.MustChangePassword);
        Assert.True(entity.MustChangePassword);
        Assert.True(PasswordHelper.Verify("Init@1234", entity.PasswordHash));
    }

    [Fact]
    public async Task ChangePasswordAsync_変更時はMustChangePasswordがfalseになる()
    {
        var entity = CreateTestEntity("user1", "test@example.com", "Test User");
        entity.MustChangePassword = true;
        _repository.GetByIdAsync("user1").Returns(entity);
        _repository.UpsertAsync(Arg.Any<UserEntity>()).Returns(Task.CompletedTask);

        var result = await _service.ChangePasswordAsync("user1", "Changed@123");

        Assert.NotNull(result);
        Assert.False(result.MustChangePassword);
        Assert.False(entity.MustChangePassword);
        Assert.True(PasswordHelper.Verify("Changed@123", entity.PasswordHash));
    }

    [Fact]
    public async Task ValidatePasswordPolicyAsync_ポリシー違反時はメッセージを返す()
    {
        var result = await _service.ValidatePasswordPolicyAsync("password");

        Assert.Equal("パスワードには英大文字を 1 文字以上含めてください。", result);
    }

    private static UserEntity CreateTestEntity(string id, string email, string displayName) =>
        new()
        {
            RowKey = id,
            Email = email,
            DisplayName = displayName,
            PasswordHash = string.Empty,
            StoreCode = "001",
            StoreName = "本店",
            RolesJson = JsonHelper.SerializeRoles([Constants.Roles.General]),
            IsActive = true,
            MustChangePassword = false,
            CreatedAt = DateTime.UtcNow,
        };
}
