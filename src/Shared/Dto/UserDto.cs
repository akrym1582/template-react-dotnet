namespace Shared.Dto;

public record UserDto(
    string UserId,
    string Email,
    string DisplayName,
    string StoreCode,
    string StoreName,
    IReadOnlyList<string> Roles,
    bool IsActive,
    bool MustChangePassword);
