namespace Shared.Dto;

/// <summary>
/// テストログイン用のユーザー情報（開発環境専用）。
/// </summary>
/// <param name="UserId">テストユーザーの ID。</param>
/// <param name="Roles">テストユーザーに割り当てられたロールの一覧。</param>
public record TestLoginUserDto(string UserId, IReadOnlyList<string> Roles);
