namespace Shared.Dto;

public record PasswordResetResultDto(string InitialPassword, bool MustChangePassword);
