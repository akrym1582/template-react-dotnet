import { useAuth } from '@/hooks/useAuth'
import { alert } from '@/lib/alert'
import { Link } from 'react-router-dom'
import { canManageUsers, formatRole } from '@/lib/user'
import { Button, buttonVariants } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { cn } from '@/lib/utils'

/**
 * ホーム画面コンポーネント。
 * ログイン中ユーザーの基本情報（表示名・メール・ロール）を表示する。
 * ユーザー管理権限を持つ場合はユーザー一覧へのリンクを表示する。
 */
export default function HomePage() {
  const { user, logout } = useAuth()

  /**
   * ログアウトボタンのクリックハンドラ。
   * 確認ダイアログを表示してからログアウト API を呼び出す。
   */
  const handleLogout = async () => {
    const confirmed = await alert.confirm('ログアウトしますか？')
    if (confirmed) {
      await logout()
      await alert.success('ログアウトしました。')
    }
  }

  return (
    <div className="container mx-auto p-8">
      <Card>
        <CardHeader>
          <CardTitle>ダッシュボード</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-muted-foreground">
            ようこそ、<span className="font-semibold text-foreground">{user?.displayName}</span>
            さん
          </p>
          <div className="space-y-2 text-sm">
            <p>
              <span className="font-medium">メール:</span> {user?.email}
            </p>
            <p>
              <span className="font-medium">ロール:</span> {user?.roles.map(formatRole).join(', ')}
            </p>
          </div>
          <div className="flex flex-col gap-3 sm:flex-row">
            <Link to={`/users/${user?.userId}`} className={cn(buttonVariants({ variant: 'outline' }))}>
              マイユーザー詳細
            </Link>
            {user && canManageUsers(user.roles) && (
              <Link to="/users" className={cn(buttonVariants({ variant: 'outline' }))}>
                ユーザー一覧
              </Link>
            )}
            <Button variant="destructive" onClick={handleLogout}>
              ログアウト
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
