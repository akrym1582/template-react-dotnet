# Template React + .NET

React 19 + ASP.NET 10 フルスタックテンプレートプロジェクト

> **ドキュメントメンテナンスルール**: 技術スタック・アーキテクチャ・規約・開発手順を変更した場合は、このファイル (`README.md`) と [`.github/copilot-instructions.md`](.github/copilot-instructions.md) の**両方**を必ず更新してください。

## プロジェクト構成

```
├── TemplateApp.slnx          # ソリューションファイル
├── src/
│   ├── Shared/                # 共有ライブラリ (Shared.csproj)
│   │   ├── Dto/               # データ転送オブジェクト
│   │   ├── Models/            # エンティティモデル (Azure Table Storage)
│   │   ├── Repository/        # リポジトリ層
│   │   ├── Services/          # ビジネスロジック層
│   │   └── Util/              # ユーティリティ
│   └── WebApp/                # Web API (WebApp.csproj)
│       ├── Controllers/       # API コントローラー
│       └── clientapp/         # React クライアント
│           ├── src/
│           │   ├── components/ui/  # shadcn/ui コンポーネント
│           │   ├── hooks/          # React Hooks (useAuth, useApi)
│           │   ├── lib/            # ユーティリティ (alert, aspida, utils)
│           │   ├── pages/          # ページコンポーネント
│           │   └── api/            # aspida 生成 API 型定義
│           └── ...
└── tests/                     # xUnit テスト (Tests.csproj)
```

## 技術スタック

### バックエンド
- **ASP.NET 10** - Web API
- **Azure Table Storage** - ユーザー情報管理
- **Azure Blob Storage** / **Queue Storage** / **Cosmos DB** - インフラ
- **Cookie認証** + **Azure Entra ID** (JWT → Cookie)

### フロントエンド
- **React 19** + **TypeScript**
- **Vite** - ビルドツール
- **TailwindCSS 4** - スタイリング
- **shadcn/ui** - UIコンポーネント
- **oxlint** - リンター
- **aspida** + **SWR** - 型安全なAPI呼び出し
- **SweetAlert2** - ポップアップアラート/確認ダイアログ

## 開発方法

### 前提条件
- .NET 10 SDK
- Node.js 20+
- Azure Storage Emulator (Azurite)

### バックエンド起動
```bash
cd src/WebApp
dotnet run
```

### フロントエンド起動 (開発サーバー)
```bash
cd src/WebApp/clientapp
npm install
npm run dev
```

WebApp (ポート5000) が Vite 開発サーバー (ポート5173) にプロキシ転送します。
同一ドメインでクライアントとAPIが動作します。

### API クライアント生成 (aspida)
```bash
# WebApp を起動した状態で:
cd src/WebApp/clientapp
npm run generate-api
```

### テスト実行
```bash
dotnet test
```

### リント
```bash
cd src/WebApp/clientapp
npm run lint
```

## 認証フロー

### パスワード認証
1. ユーザーがメール/パスワードで `POST /api/auth/login`
2. 認証成功で Cookie セッション発行
3. 以降 Cookie で認証

### 開発環境向けテストログイン
- `src/WebApp/appsettings.json` の `TestLogin:Users` にユーザーIDとロールを複数定義できます
- 開発環境では `GET /api/auth/test-users` でテストユーザー一覧を取得できます
- 開発環境では `POST /api/auth/test-login` に `userId` を送るとパスワードなしで Cookie セッションを発行できます
- ログイン画面には設定済みのテストユーザーがボタン表示されます
- テストログインのロールは `general` / `manager` / `privileged` を使用します

### Azure Entra ID 認証
1. クライアントで Entra ID から JWT トークン取得
2. `POST /api/auth/entra-login` で JWT を送信
3. サーバーで JWT 検証、ユーザー自動作成/取得
4. Cookie セッション発行
5. 以降 Cookie で認証

## ユーザー管理

- Azure Table Storage に保存 (PartitionKey: "USER", RowKey: UserId)
- ユーザー一覧では店番プルダウンで絞り込み表示できます
- ユーザー詳細ではユーザーGUIDを固定IDとして表示し、表示名・ユーザーID（メールアドレス）を更新できます
- ロールは `general` / `manager` / `privileged` を `RolesJson` に保存します
- `manager` 以上は他ユーザー詳細とロールを更新できます
- `UserManagement:AllowManagerUserCreation` が `true` のとき、`manager` 以上はユーザー追加できます
- パスワード初期化文字列とポリシーは `UserManagement` セクションで設定します
