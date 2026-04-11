namespace Shared.Dto;

public record UserDto(
    string UserId,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    bool IsActive);
