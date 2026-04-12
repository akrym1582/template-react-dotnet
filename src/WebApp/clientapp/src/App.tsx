import { useAuth } from '@/hooks/useAuth'
import { Navigate, Route, Routes } from 'react-router-dom'
import LoginPage from '@/pages/LoginPage'
import HomePage from '@/pages/HomePage'
import UserListPage from '@/pages/UserListPage'
import UserDetailPage from '@/pages/UserDetailPage'
import ChangePasswordPage from '@/pages/ChangePasswordPage'

/**
 * アプリケーションのルートコンポーネント。
 * 認証状態に応じてルーティングを制御する。
 * - 未ログイン: ログインページのみ表示
 * - 初回パスワード変更必須: パスワード変更ページのみ表示
 * - ログイン済み: 通常のルーティングを適用
 */
function App() {
  const { user, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p className="text-muted-foreground">読み込み中...</p>
      </div>
    )
  }

  if (!user) {
    return (
      <Routes>
        <Route path="*" element={<LoginPage />} />
      </Routes>
    )
  }

  if (user.mustChangePassword) {
    return <ChangePasswordPage />
  }

  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/users" element={<UserListPage />} />
      <Route path="/users/:userId" element={<UserDetailPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App
