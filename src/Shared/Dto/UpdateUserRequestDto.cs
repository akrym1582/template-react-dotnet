namespace Shared.Dto;

public record UpdateUserRequestDto(
    string Email,
    string DisplayName,
    string StoreCode,
    string StoreName,
    IReadOnlyList<string>? Roles);
