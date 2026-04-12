import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
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
 * ユーザー詳細ページコンポーネント。
 * URL パラメータ `:userId` に対応するユーザーの詳細情報を表示・編集する。
 * パラメータが省略された場合はログイン中ユーザー自身の情報を表示する。
 * ユーザー管理権限を持つ場合は店番・店名・ロールの編集も可能。
 * 自分自身のユーザーに対してはパスワード初期化ボタンを表示する。
 */
export default function UserDetailPage() {
  const params = useParams()
  const { user, logout, resetPassword } = useAuth()
  const [detailUser, setDetailUser] = useState<UserDto>()
  const [email, setEmail] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [storeCode, setStoreCode] = useState('')
  const [storeName, setStoreName] = useState('')
  const [role, setRole] = useState('general')
  const [isLoading, setIsLoading] = useState(true)

  const targetUserId = params.userId ?? user?.userId
  const canManage = user ? canManageUsers(user.roles) : false
  const isSelf = user?.userId === detailUser?.userId

  useEffect(() => {
    /** 対象ユーザーの詳細情報を API から取得してフォームに反映する */
    const loadUser = async () => {
      if (!targetUserId) {
        setIsLoading(false)
        return
      }

      setIsLoading(true)
      const response = await apiFetch(`/api/user/${targetUserId}`)
      const json: ApiResponse<UserDto> = await response.json()

      if (!json.success || !json.data) {
        await alert.error(json.message ?? 'ユーザー情報の取得に失敗しました。')
        setIsLoading(false)
        return
      }

      setDetailUser(json.data)
      setEmail(json.data.email)
      setDisplayName(json.data.displayName)
      setStoreCode(json.data.storeCode)
      setStoreName(json.data.storeName)
      setRole(json.data.roles[0] ?? 'general')
      setIsLoading(false)
    }

    void loadUser()
  }, [targetUserId])

  /**
   * ユーザー情報更新フォームの送信ハンドラ。
   * 入力内容でユーザー情報を更新する。
   * @param event - フォーム送信イベント
   */
  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!targetUserId) return

    const response = await apiFetch(`/api/user/${targetUserId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        email,
        displayName,
        storeCode,
        storeName,
        roles: [role],
      }),
    })

    const json: ApiResponse<UserDto> = await response.json()
    if (!json.success || !json.data) {
      await alert.error(json.message ?? 'ユーザー情報の更新に失敗しました。')
      return
    }

    setDetailUser(json.data)
    await alert.success('ユーザー情報を更新しました。')
  }

  /**
   * パスワード初期化ボタンのクリックハンドラ。
   * 確認ダイアログを表示してからパスワードを初期化し、
   * 初期パスワードをクリップボードへコピーしてログアウトする。
   */
  const handleResetPassword = async () => {
    const confirmed = await alert.confirm('パスワードを初期化しますか？')
    if (!confirmed) return

    const result = await resetPassword()
    if (!result.success) {
      await alert.error(result.message ?? 'パスワードの初期化に失敗しました。')
      return
    }

    await logout()
    await notifyInitialPassword(
      result.data?.initialPassword,
      '初期パスワードをクリップボードにコピーしました。再ログイン後に変更してください。',
      'パスワードを初期化しました。初期パスワードは設定値 UserManagement:InitialPassword を確認してください。',
    )
  }

  if (isLoading) {
    return (
      <div className="container mx-auto p-8">
        <p className="text-sm text-muted-foreground">読み込み中...</p>
      </div>
    )
  }

  if (!detailUser) {
    return (
      <div className="container mx-auto p-8">
        <p className="text-sm text-muted-foreground">ユーザー情報を表示できません。</p>
      </div>
    )
  }

  return (
    <div className="container mx-auto space-y-6 p-8">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between gap-4">
          <CardTitle>ユーザー詳細</CardTitle>
          <div className="flex gap-2">
            {canManage && (
              <Link to="/users" className={cn(buttonVariants({ variant: 'outline' }))}>
                一覧へ戻る
              </Link>
            )}
            <Link to="/" className={cn(buttonVariants({ variant: 'outline' }))}>
              ホームに戻る
            </Link>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2 text-sm">
            <p>
              <span className="font-medium">ユーザーGUID:</span> {detailUser.userId}
            </p>
            <p>
              <span className="font-medium">現在のロール:</span> {detailUser.roles.map(formatRole).join(', ')}
            </p>
          </div>

          <form onSubmit={(event) => void handleSubmit(event)} className="space-y-4">
            <div className="space-y-2">
              <label htmlFor="displayName" className="text-sm font-medium">
                表示名
              </label>
              <Input
                id="displayName"
                value={displayName}
                onChange={(event) => setDisplayName(event.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="email" className="text-sm font-medium">
                ユーザーID（メールアドレス）
              </label>
              <Input
                id="email"
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
              />
            </div>

            {canManage && (
              <>
                <div className="space-y-2">
                  <label htmlFor="storeCode" className="text-sm font-medium">
                    店番
                  </label>
                  <Input
                    id="storeCode"
                    value={storeCode}
                    onChange={(event) => setStoreCode(event.target.value)}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <label htmlFor="storeName" className="text-sm font-medium">
                    店名
                  </label>
                  <Input
                    id="storeName"
                    value={storeName}
                    onChange={(event) => setStoreName(event.target.value)}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <label htmlFor="role" className="text-sm font-medium">
                    ロール
                  </label>
                  <select
                    id="role"
                    className="flex h-10 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs"
                    value={role}
                    onChange={(event) => setRole(event.target.value)}
                  >
                    {roleOptions.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </div>
              </>
            )}

            <div className="flex flex-col gap-3 sm:flex-row">
              <Button type="submit">保存</Button>
              {isSelf && (
                <Button type="button" variant="secondary" onClick={() => void handleResetPassword()}>
                  自分のパスワードを初期化
                </Button>
              )}
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
