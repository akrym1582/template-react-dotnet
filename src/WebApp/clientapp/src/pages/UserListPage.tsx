import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import type { UserDto } from '@/hooks/useAuth'
import { alert } from '@/lib/alert'
import { apiFetch } from '@/lib/aspida'
import { notifyInitialPassword } from '@/lib/password'
import { canManageUsers, formatRole, roleOptions } from '@/lib/user'
import { Button, buttonVariants } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/utils'

/** ユーザー一覧取得 API のレスポンス型 */
interface UserListResponseDto {
  /** ユーザー一覧 */
  users: UserDto[]
  /** 新規ユーザー追加が許可されているかどうか */
  allowUserCreation: boolean
}

/** 汎用 API レスポンス型（ページローカル用） */
interface ApiResponse<T> {
  /** 処理の成否 */
  success: boolean
  /** レスポンスデータ */
  data?: T
  /** エラーメッセージなどの補足メッセージ */
  message?: string
}

/**
 * ユーザー一覧ページコンポーネント。
 * 全ユーザーを店番でフィルタリングして表示する。
 * ユーザー管理権限があり新規作成が許可されている場合、ユーザー追加フォームも表示する。
 */
export default function UserListPage() {
  const { user } = useAuth()
  const [users, setUsers] = useState<UserDto[]>([])
  const [allowUserCreation, setAllowUserCreation] = useState(false)
  const [selectedStoreCode, setSelectedStoreCode] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [createEmail, setCreateEmail] = useState('')
  const [createDisplayName, setCreateDisplayName] = useState('')
  const [createStoreCode, setCreateStoreCode] = useState('')
  const [createStoreName, setCreateStoreName] = useState('')
  const [createRole, setCreateRole] = useState('general')

  /** ユーザー一覧を API から取得してステートを更新する */
  const loadUsers = async () => {
    setIsLoading(true)
    try {
      const response = await apiFetch('/api/user')
      const json: ApiResponse<UserListResponseDto> = await response.json()

      if (!json.success) {
        await alert.error(json.message ?? 'ユーザー一覧の取得に失敗しました。')
        return
      }

      setUsers(json.data?.users ?? [])
      setAllowUserCreation(json.data?.allowUserCreation ?? false)
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadUsers()
  }, [])

  /** ユーザー一覧から店番の重複を排除して店番一覧を生成する */
  const stores = useMemo(() => {
    const uniqueStores = new Map<string, string>()

    users.forEach((targetUser) => {
      if (targetUser.storeCode && !uniqueStores.has(targetUser.storeCode)) {
        uniqueStores.set(targetUser.storeCode, targetUser.storeName)
      }
    })

    return Array.from(uniqueStores.entries()).map(([storeCode, storeName]) => ({
      storeCode,
      storeName,
    }))
  }, [users])

  useEffect(() => {
    if (!selectedStoreCode && stores.length > 0) {
      setSelectedStoreCode(stores[0].storeCode)
    }
  }, [selectedStoreCode, stores])

  const effectiveSelectedStoreCode = selectedStoreCode || stores[0]?.storeCode || ''
  const filteredUsers = effectiveSelectedStoreCode
    ? users.filter((targetUser) => targetUser.storeCode === effectiveSelectedStoreCode)
    : users

  /**
   * ユーザー追加フォームの送信ハンドラ。
   * 入力内容で新規ユーザーを作成し、初期パスワードをクリップボードへコピーする。
   * @param event - フォーム送信イベント
   */
  const handleCreate = async (event: FormEvent) => {
    event.preventDefault()

    const response = await apiFetch('/api/user', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        email: createEmail,
        displayName: createDisplayName,
        storeCode: createStoreCode,
        storeName: createStoreName,
        roles: [createRole],
      }),
    })

    const json: ApiResponse<{ initialPassword: string }> = await response.json()
    if (!json.success) {
      await alert.error(json.message ?? 'ユーザーの追加に失敗しました。')
      return
    }

    await notifyInitialPassword(
      json.data?.initialPassword,
      'ユーザーを追加しました。初期パスワードをクリップボードにコピーしました。',
      'ユーザーを追加しました。初期パスワードは設定値 UserManagement:InitialPassword を確認してください。',
    )
    setCreateEmail('')
    setCreateDisplayName('')
    setCreateStoreCode('')
    setCreateStoreName('')
    setCreateRole('general')
    await loadUsers()
  }

  return (
    <div className="container mx-auto space-y-6 p-8">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between gap-4">
          <CardTitle>ユーザー一覧</CardTitle>
          <Link to="/" className={cn(buttonVariants({ variant: 'outline' }))}>
            ホームに戻る
          </Link>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <label htmlFor="storeCode" className="text-sm font-medium">
              店番
            </label>
            <select
              id="storeCode"
              className="flex h-10 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs"
              value={effectiveSelectedStoreCode}
              onChange={(event) => setSelectedStoreCode(event.target.value)}
            >
              {stores.map((store) => (
                <option key={store.storeCode} value={store.storeCode}>
                  {store.storeCode} {store.storeName}
                </option>
              ))}
            </select>
          </div>

          {isLoading ? (
            <p className="text-sm text-muted-foreground">読み込み中...</p>
          ) : filteredUsers.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left">
                    <th className="px-2 py-3">表示名</th>
                    <th className="px-2 py-3">ユーザーID</th>
                    <th className="px-2 py-3">店番</th>
                    <th className="px-2 py-3">ロール</th>
                    <th className="px-2 py-3" />
                  </tr>
                </thead>
                <tbody>
                  {filteredUsers.map((targetUser) => (
                    <tr key={targetUser.userId} className="border-b">
                      <td className="px-2 py-3">{targetUser.displayName}</td>
                      <td className="px-2 py-3">{targetUser.email}</td>
                      <td className="px-2 py-3">
                        {targetUser.storeCode} {targetUser.storeName}
                      </td>
                      <td className="px-2 py-3">{targetUser.roles.map(formatRole).join(', ')}</td>
                      <td className="px-2 py-3 text-right">
                        <Link
                          to={`/users/${targetUser.userId}`}
                          className={cn(buttonVariants({ size: 'sm', variant: 'outline' }))}
                        >
                          詳細
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">選択した店番のユーザーはいません。</p>
          )}
        </CardContent>
      </Card>

      {user && canManageUsers(user.roles) && allowUserCreation && (
        <Card>
          <CardHeader>
            <CardTitle>ユーザー追加</CardTitle>
          </CardHeader>
          <CardContent>
            <form className="grid gap-4 md:grid-cols-2" onSubmit={(event) => void handleCreate(event)}>
              <div className="space-y-2">
                <label htmlFor="createDisplayName" className="text-sm font-medium">
                  表示名
                </label>
                <Input
                  id="createDisplayName"
                  value={createDisplayName}
                  onChange={(event) => setCreateDisplayName(event.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <label htmlFor="createEmail" className="text-sm font-medium">
                  ユーザーID（メールアドレス）
                </label>
                <Input
                  id="createEmail"
                  type="email"
                  value={createEmail}
                  onChange={(event) => setCreateEmail(event.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <label htmlFor="createStoreCode" className="text-sm font-medium">
                  店番
                </label>
                <Input
                  id="createStoreCode"
                  value={createStoreCode}
                  onChange={(event) => setCreateStoreCode(event.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <label htmlFor="createStoreName" className="text-sm font-medium">
                  店名
                </label>
                <Input
                  id="createStoreName"
                  value={createStoreName}
                  onChange={(event) => setCreateStoreName(event.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <label htmlFor="createRole" className="text-sm font-medium">
                  ロール
                </label>
                <select
                  id="createRole"
                  className="flex h-10 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs"
                  value={createRole}
                  onChange={(event) => setCreateRole(event.target.value)}
                >
                  {roleOptions.map((role) => (
                    <option key={role.value} value={role.value}>
                      {role.label}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex items-end">
                <Button type="submit" className="w-full">
                  ユーザーを追加
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
