namespace Shared.Dto;

/// <summary>
/// Azure Entra ID ログインリクエスト。
/// クライアント側で取得した JWT トークンを送信し、サーバーがクッキーセッションを発行する。
/// </summary>
/// <param name="IdToken">Azure Entra ID から取得した ID トークン（JWT 形式）。</param>
public record EntraLoginRequestDto(string IdToken);
