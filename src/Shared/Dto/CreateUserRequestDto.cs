namespace Shared.Dto;

public record CreateUserRequestDto(
    string Email,
    string DisplayName,
    string StoreCode,
    string StoreName,
    IReadOnlyList<string>? Roles);
