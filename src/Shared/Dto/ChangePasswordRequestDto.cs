namespace Shared.Dto;

/// <summary>
/// パスワード変更リクエスト。
/// </summary>
/// <param name="NewPassword">新しいパスワード。</param>
public record ChangePasswordRequestDto(string NewPassword);
