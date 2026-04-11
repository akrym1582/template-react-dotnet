namespace Shared.Dto;

public record CreatedUserResultDto(
    UserDto User,
    string InitialPassword,
    bool MustChangePassword);
