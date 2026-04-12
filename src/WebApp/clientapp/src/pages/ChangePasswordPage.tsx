import { useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '@/hooks/useAuth'
import { alert } from '@/lib/alert'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

/**
 * パスワード変更ページコンポーネント。
 * 初回ログイン時など `mustChangePassword` が `true` の場合に表示される。
 * 新しいパスワードと確認用パスワードを入力してパスワードを変更する。
 */
export default function ChangePasswordPage() {
  const { changePassword } = useAuth()
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  /**
   * フォーム送信ハンドラ。
   * 確認用パスワードの一致を検証し、パスワード変更 API を呼び出す。
   * @param event - フォーム送信イベント
   */
  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (isSubmitting) return

    if (newPassword !== confirmPassword) {
      await alert.error('確認用パスワードが一致しません。')
      return
    }

    setIsSubmitting(true)
    try {
      const result = await alert.withLoading(() => changePassword(newPassword))
      if (!result?.success) {
        await alert.error(result?.message ?? 'パスワードの変更に失敗しました。')
        return
      }

      await alert.success('パスワードを変更しました。')
      setNewPassword('')
      setConfirmPassword('')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>パスワード変更</CardTitle>
          <CardDescription>
            初期化後の初回ログインです。続行するには新しいパスワードへ変更してください。
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <label htmlFor="newPassword" className="text-sm font-medium">
                新しいパスワード
              </label>
              <Input
                id="newPassword"
                type="password"
                value={newPassword}
                onChange={(event) => setNewPassword(event.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <label htmlFor="confirmPassword" className="text-sm font-medium">
                新しいパスワード（確認）
              </label>
              <Input
                id="confirmPassword"
                type="password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                required
              />
            </div>
            <Button type="submit" className="w-full" disabled={isSubmitting}>
              {isSubmitting ? '変更中...' : 'パスワードを変更'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
