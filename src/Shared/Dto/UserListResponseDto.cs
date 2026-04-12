namespace Shared.Dto;

/// <summary>
/// ユーザー一覧レスポンス。
/// </summary>
/// <param name="Users">ユーザーの一覧。</param>
/// <param name="AllowUserCreation">マネージャーによるユーザー作成が許可されているかどうか。</param>
public record UserListResponseDto(
    IReadOnlyList<UserDto> Users,
    bool AllowUserCreation);
