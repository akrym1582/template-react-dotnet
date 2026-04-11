namespace Shared.Dto;

public record TestLoginUserDto(string UserId, IReadOnlyList<string> Roles);
