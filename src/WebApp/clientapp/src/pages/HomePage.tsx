import { useAuth } from '@/hooks/useAuth'
import { alert } from '@/lib/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

export default function HomePage() {
  const { user, logout } = useAuth()

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
              <span className="font-medium">ロール:</span> {user?.roles.join(', ')}
            </p>
          </div>
          <Button variant="destructive" onClick={handleLogout}>
            ログアウト
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
