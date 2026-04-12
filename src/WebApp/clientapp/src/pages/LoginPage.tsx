import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import api from '@/api/$api'
import type { TestLoginUserDto } from '@/hooks/useAuth'
import { useAuth } from '@/hooks/useAuth'
import { alert } from '@/lib/alert'
import { asApiResponse } from '@/lib/apiResponse'
import { aspidaClientNoThrow } from '@/lib/aspida'
import { notifyInitialPassword } from '@/lib/password'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

const authApi = api(aspidaClientNoThrow).api.Auth

/**
 * ログインページコンポーネント。
 * メールアドレスとパスワードによるログインフォームを表示する。
 * 開発環境ではテストログインユーザー一覧も表示する。
 * Azure Entra ID によるログインボタンも配置するが現在は無効。
 */
export default function LoginPage() {
  const { login, testLogin, resetPasswordByCredentials } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [testLoginUsers, setTestLoginUsers] = useState<TestLoginUserDto[]>([])
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    let isMounted = true

    /** テストログイン可能なユーザー一覧を API から取得する */
    const loadTestLoginUsers = async () => {
      const result = await authApi.test_users.get()

      if (!result.originalResponse.ok) {
        return
      }

      const response = asApiResponse<TestLoginUserDto[]>(result.body)
      if (isMounted && response.success) {
        setTestLoginUsers(response.data ?? [])
      }
    }

    void loadTestLoginUsers().catch(() => {
      if (isMounted) {
        setTestLoginUsers([])
      }
    })

    return () => {
      isMounted = false
    }
  }, [])

  /**
   * ログインフォームの送信ハンドラ。
   * 入力されたメールアドレスとパスワードでログイン API を呼び出す。
   * @param e - フォーム送信イベント
   */
  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (isSubmitting) return

    setIsSubmitting(true)
    try {
      const result = await alert.withLoading(() => login(email, password))
      if (result && !result.success) {
        await alert.error(result.message ?? 'ログインに失敗しました。')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  /**
   * テストログインボタンのクリックハンドラ。
   * 指定したユーザー ID でテストログイン API を呼び出す（開発環境専用）。
   * @param userId - テストログインするユーザー ID
   */
  const handleTestLogin = async (userId: string) => {
    if (isSubmitting) return

    setIsSubmitting(true)
    try {
      const result = await alert.withLoading(() => testLogin(userId))
      if (result && !result.success) {
        await alert.error(result.message ?? 'テストログインに失敗しました。')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  /**
   * パスワード初期化ボタンのクリックハンドラ。
   * 入力されたメールアドレスと現在のパスワードでパスワードを初期化し、
   * 初期パスワードをクリップボードへコピーする。
   */
  const handleResetPassword = async () => {
    if (isSubmitting) return

    setIsSubmitting(true)
    try {
      const result = await alert.withLoading(() => resetPasswordByCredentials(email, password))
      if (result && !result.success) {
        await alert.error(result.message ?? 'パスワードの初期化に失敗しました。')
        return
      }

      await notifyInitialPassword(
        result?.data?.initialPassword,
        '初期パスワードをクリップボードにコピーしました。再ログイン後に変更してください。',
        'パスワードを初期化しました。初期パスワードは設定値 UserManagement:InitialPassword を確認してください。',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>ログイン</CardTitle>
          <CardDescription>メールアドレスとパスワードでログインしてください。</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <label htmlFor="email" className="text-sm font-medium">
                メールアドレス
              </label>
              <Input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="user@example.com"
                required
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="password" className="text-sm font-medium">
                パスワード
              </label>
              <Input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            <Button type="submit" className="w-full" disabled={isSubmitting}>
              {isSubmitting ? 'ログイン中...' : 'ログイン'}
            </Button>
            <Button
              type="button"
              variant="outline"
              className="w-full"
              disabled={isSubmitting}
              onClick={() => void handleResetPassword()}
            >
              現在のパスワードで初期化
            </Button>
            <p className="text-xs text-muted-foreground">
              ログイン画面からの初期化では、メールアドレスと現在のパスワードの入力が必要です。
            </p>
          </form>

          {testLoginUsers.length > 0 && (
            <div className="mt-6 space-y-3">
              <div className="relative">
                <div className="absolute inset-0 flex items-center">
                  <span className="w-full border-t" />
                </div>
                <div className="relative flex justify-center text-xs uppercase">
                  <span className="bg-background px-2 text-muted-foreground">テストログイン</span>
                </div>
              </div>
              <div className="space-y-2">
                {testLoginUsers.map((user) => (
                  <Button
                    key={user.userId}
                    type="button"
                    variant="secondary"
                    className="w-full justify-between"
                    disabled={isSubmitting}
                    onClick={() => void handleTestLogin(user.userId)}
                  >
                    <span>{user.userId}</span>
                    <span className="text-xs text-muted-foreground">{user.roles.join(', ')}</span>
                  </Button>
                ))}
              </div>
            </div>
          )}

          <div className="mt-4">
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <span className="w-full border-t" />
              </div>
              <div className="relative flex justify-center text-xs uppercase">
                <span className="bg-background px-2 text-muted-foreground">または</span>
              </div>
            </div>
            <Button variant="outline" className="mt-4 w-full" disabled>
              Azure Entra ID でログイン
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
