# Copilot Instructions

このファイルはプロジェクトの規約・パターン・ベストプラクティスを記述しています。
コードを生成・レビューする際は必ずこのファイルの内容に従ってください。

> **メンテナンスルール**: コードの構造・技術スタック・規約を変更した場合は、このファイルと `README.md` の両方を必ず更新してください。

---

## プロジェクト概要

React 19 + ASP.NET 10 のフルスタックテンプレートプロジェクトです。

```
TemplateApp.slnx
├── src/
│   ├── Shared/          # 共有ライブラリ (Shared.csproj)
│   └── WebApp/          # Web API + React SPA (WebApp.csproj)
│       └── clientapp/   # React フロントエンド
└── tests/               # xUnit テスト (Tests.csproj)
```

---

## バックエンド (ASP.NET 10)

### アーキテクチャ

- **レイヤー構成**: Controller → Service → Repository → Azure Table Storage
- 各レイヤーにインターフェースを定義し、DI コンテナに登録する
- `Shared` プロジェクトに DTO / Models / Repository / Services / Util を配置する
- `WebApp` プロジェクトに Controllers を配置する

### コントローラー規約

```csharp
[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    private readonly ISampleService _sampleService;

    public SampleController(ISampleService sampleService)
    {
        _sampleService = sampleService;
    }
}
```

- すべてのエンドポイントは `ActionResult<ApiResponseDto<T>>` または `ActionResult<ApiResponseDto>` を返す
- 認証が必要なエンドポイントには `[Authorize]` を付与する
- 管理者専用エンドポイントには `[Authorize(Roles = "admin")]` を付与する
- 認証失敗は `Unauthorized(new ApiResponseDto(false, "メッセージ"))` を返す
- リソース未発見は `NotFound(new ApiResponseDto(false, "メッセージ"))` を返す
- 成功は `Ok(new ApiResponseDto<T>(true, data))` を返す

### サービス規約

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserDto?> GetByIdAsync(string userId)
    {
        var entity = await _repository.GetByIdAsync(userId);
        return entity is null ? null : ToDto(entity);
    }
}
```

- すべての公開メソッドは `async Task<T>` を使う
- `null` チェックは `is null` / `is not null` パターンマッチングを使う
- Entity → DTO 変換は `private static ToDto(entity)` メソッドで行う

### リポジトリ規約

- インターフェース (`IUserRepository`) をサービス層で依存する
- Azure Table Storage を使用: `PartitionKey` は定数 (`Constants.UserPartitionKey`)、`RowKey` は ID

### DTO 規約

```csharp
// Shared.Dto に record で定義
public record UserDto(
    string UserId,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    bool IsActive);

// 統一 API レスポンス型
public record ApiResponseDto<T>(bool Success, T? Data = default, string? Message = null);
public record ApiResponseDto(bool Success, string? Message = null);
```

- DTO は `record` で定義する
- `Shared.Dto` 名前空間に配置する

### 認証

- Cookie セッション + Azure Entra ID (JWT → Cookie) の 2 通りをサポート
- ローカルログイン: `POST /api/auth/login` (email/password → Cookie)
- Entra ID ログイン: `POST /api/auth/entra-login` (idToken → Cookie)
- Cookie 名は `Constants.AuthCookieName` を参照する

### 定数

- ロール: `Constants.Roles.General` (`"general"`), `Constants.Roles.Manager` (`"manager"`), `Constants.Roles.Privileged` (`"privileged"`)
- テーブル名: `Constants.UsersTableName`
- Cookie 名: `Constants.AuthCookieName`
- `UserManagement` セクションで初期パスワード、パスワードポリシー、役席以上のユーザー追加可否を設定する

### DI 登録 (`Program.cs`)

```csharp
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUserService, UserService>();
```

### NuGet パッケージ管理

- .NET のパッケージ バージョンはリポジトリ ルートの `Directory.Packages.props` で中央管理する
- 各 `.csproj` の `PackageReference` には `Version` を直書きしない
- C# プロジェクトには `StyleCop.Analyzers` を共通適用し、`SA1101` / `SA1200` / `SA1309` / `SA1629` / `SA1633` は無効化する
- 開発時の SPA 統合は SpaProxy を使い、`UseSpa` / `UseProxyToSpaDevelopmentServer` は使わず `WebApp.csproj` と `launchSettings.json` で設定する

---

## フロントエンド (React 19 + TypeScript)

### 技術スタック

| ツール | 用途 |
|--------|------|
| Vite | ビルド・開発サーバー |
| TailwindCSS 4 | スタイリング |
| shadcn/ui | UI コンポーネント |
| aspida + @aspida/swr | 型安全 API 呼び出し + データフェッチ |
| SweetAlert2 | ポップアップ・確認ダイアログ |
| oxlint | リンター |
| vitest + @testing-library/react | テスト |

### パスエイリアス

`@/` は `src/` のエイリアス。

```typescript
import { useAuth } from '@/hooks/useAuth'
import { Button } from '@/components/ui/button'
import { alert } from '@/lib/alert'
```

### コンポーネント規約

- `export default` でエクスポートする
- shadcn/ui コンポーネント (`@/components/ui/`) を優先して使用する
- 新しい shadcn/ui コンポーネントが必要な場合: `npx shadcn@latest add <component>`

```tsx
export default function SamplePage() {
  return (
    <div className="container mx-auto p-8">
      <Card>
        <CardHeader>
          <CardTitle>タイトル</CardTitle>
        </CardHeader>
        <CardContent>
          {/* コンテンツ */}
        </CardContent>
      </Card>
    </div>
  )
}
```

### データフェッチ

**API クライアント (aspida)**:
```typescript
import api from '@/api/$api'
import { aspidaClient } from '@/lib/aspida'

const sampleApi = api(aspidaClient).sample
```

**SWR フック**:
```typescript
import { useApi } from '@/hooks/useApi'

const { data, error, isLoading } = useApi<SomeType>(path)
```

- SWR を使うときは `swr` の `useSWR` ではなく `@aspida/swr` の `useAspidaSWR` を使う
- API フェッチは原則 `credentials: 'same-origin'` で Cookie 認証情報を送信する（ログインなど明示的な例外を除く）

**認証状態**:
```typescript
import { useAuth } from '@/hooks/useAuth'

const { user, isLoading, login, logout, entraLogin } = useAuth()
```

### ダイアログ・アラート

`@/lib/alert` を使用する。直接 SweetAlert2 を呼び出さない。

```typescript
import { alert } from '@/lib/alert'

await alert.success('保存しました。')
await alert.error('エラーが発生しました。')
const ok = await alert.confirm('削除しますか？')
const result = await alert.withLoading(() => someAsyncAction())
```

### API クライアント生成

```bash
# WebApp を起動した状態で:
cd src/WebApp/clientapp
npm run generate-api
```

aspida が `src/api/` 配下を自動生成するため、このディレクトリは手動編集しない。

---

## テスト

### バックエンド (xUnit + NSubstitute)

```csharp
public class UserServiceTests
{
    private readonly IUserRepository _repository;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repository = Substitute.For<IUserRepository>();
        _service = new UserService(_repository);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsDto()
    {
        // Arrange
        var entity = CreateTestEntity("user1", "test@example.com", "Test User");
        _repository.GetByIdAsync("user1").Returns(entity);

        // Act
        var result = await _service.GetByIdAsync("user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result.UserId);
    }
}
```

- テストは `tests/` に配置し、`Tests.csproj` に含める
- モックは NSubstitute を使用する
- テスト名は日本語で記述する (例: `GetByIdAsync_ExistingUser_ReturnsDto`)
- テストは `dotnet test tests/Tests.csproj` で実行する

### フロントエンド (vitest + @testing-library/react)

```tsx
import { render, screen } from '@testing-library/react'
import { vi } from 'vitest'

vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}))

describe('SampleComponent', () => {
  it('正常にレンダリングされる', () => {
    render(<SampleComponent />)
    expect(screen.getByText('テキスト')).toBeInTheDocument()
  })
})
```

- テストは `src/__tests__/` 配下に配置する (ディレクトリ構造は `src/` と対応させる)
- テストは `cd src/WebApp/clientapp && npm run test` で実行する
- テストの説明文は日本語で記述する

---

## 開発コマンド

```bash
# バックエンドビルド
dotnet build TemplateApp.slnx

# バックエンドテスト
dotnet test tests/Tests.csproj

# バックエンド起動
cd src/WebApp && dotnet run

# フロントエンドインストール
cd src/WebApp/clientapp && npm install

# フロントエンド開発サーバー
cd src/WebApp/clientapp && npm run dev

# フロントエンドビルド
cd src/WebApp/clientapp && npm run build

# フロントエンドリント
cd src/WebApp/clientapp && npm run lint

# フロントエンドテスト
cd src/WebApp/clientapp && npm run test

# フロントエンドテスト (カバレッジ)
cd src/WebApp/clientapp && npm run test:coverage
```

---

## UI テキスト・メッセージ

- UI に表示するラベル・メッセージ・エラー文はすべて日本語で記述する
- コードのコメントも日本語で記述してよい
