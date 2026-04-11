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
        _service = new UserService(_repository);
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

        var result = await _service.CreateAsync("new@example.com", "New User", "Password123!");

        Assert.NotNull(result);
        Assert.Equal("new@example.com", result.Email);
        Assert.Contains(Constants.Roles.User, result.Roles);

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

    private static UserEntity CreateTestEntity(string id, string email, string displayName) =>
        new()
        {
            RowKey = id,
            Email = email,
            DisplayName = displayName,
            PasswordHash = "",
            RolesJson = JsonHelper.SerializeRoles([Constants.Roles.User]),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
}
