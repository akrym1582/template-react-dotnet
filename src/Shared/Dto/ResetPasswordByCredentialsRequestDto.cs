namespace Shared.Dto;

public record ResetPasswordByCredentialsRequestDto(string Email, string CurrentPassword);
