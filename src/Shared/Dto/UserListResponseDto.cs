namespace Shared.Dto;

public record UserListResponseDto(
    IReadOnlyList<UserDto> Users,
    bool AllowUserCreation);
