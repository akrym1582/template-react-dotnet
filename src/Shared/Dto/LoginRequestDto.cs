namespace Shared.Dto;

/// <summary>
/// メールアドレスとパスワードによるログインリクエスト。
/// </summary>
/// <param name="Email">メールアドレス。</param>
/// <param name="Password">パスワード。</param>
public record LoginRequestDto(string Email, string Password);
