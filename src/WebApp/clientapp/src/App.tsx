import { useAuth } from '@/hooks/useAuth'
import { Navigate, Route, Routes } from 'react-router-dom'
import LoginPage from '@/pages/LoginPage'
import HomePage from '@/pages/HomePage'
import UserListPage from '@/pages/UserListPage'
import UserDetailPage from '@/pages/UserDetailPage'
import ChangePasswordPage from '@/pages/ChangePasswordPage'

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
