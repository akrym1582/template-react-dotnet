namespace Shared.Dto;

public record ApiResponseDto<T>(bool Success, T? Data = default, string? Message = null);

public record ApiResponseDto(bool Success, string? Message = null);
