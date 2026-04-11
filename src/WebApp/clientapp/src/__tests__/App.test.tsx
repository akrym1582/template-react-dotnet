import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import App from '@/App'

vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}))

vi.mock('@/pages/LoginPage', () => ({
  default: () => <div data-testid="login-page">ログインページ</div>,
}))

vi.mock('@/pages/HomePage', () => ({
  default: () => <div data-testid="home-page">ホームページ</div>,
}))

vi.mock('@/pages/UserListPage', () => ({
  default: () => <div data-testid="user-list-page">ユーザー一覧ページ</div>,
}))

vi.mock('@/pages/UserDetailPage', () => ({
  default: () => <div data-testid="user-detail-page">ユーザー詳細ページ</div>,
}))

vi.mock('@/pages/ChangePasswordPage', () => ({
  default: () => <div data-testid="change-password-page">パスワード変更ページ</div>,
}))

import { useAuth } from '@/hooks/useAuth'

const mockUseAuth = vi.mocked(useAuth)

describe('App', () => {
  it('ローディング中はローディング表示を返す', () => {
    mockUseAuth.mockReturnValue({
      user: undefined,
      isLoading: true,
      isError: false,
      login: vi.fn(),
      testLogin: vi.fn(),
      entraLogin: vi.fn(),
      logout: vi.fn(),
      changePassword: vi.fn(),
      resetPassword: vi.fn(),
      resetPasswordByCredentials: vi.fn(),
    })

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByText('読み込み中...')).toBeInTheDocument()
    expect(screen.queryByTestId('login-page')).not.toBeInTheDocument()
    expect(screen.queryByTestId('home-page')).not.toBeInTheDocument()
  })

  it('未認証の場合はログインページを表示する', () => {
    mockUseAuth.mockReturnValue({
      user: undefined,
      isLoading: false,
      isError: false,
      login: vi.fn(),
      testLogin: vi.fn(),
      entraLogin: vi.fn(),
      logout: vi.fn(),
      changePassword: vi.fn(),
      resetPassword: vi.fn(),
      resetPasswordByCredentials: vi.fn(),
    })

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByTestId('login-page')).toBeInTheDocument()
    expect(screen.queryByTestId('home-page')).not.toBeInTheDocument()
  })

  it('認証済みの場合はホームページを表示する', () => {
    mockUseAuth.mockReturnValue({
      user: {
        userId: 'user-1',
        email: 'test@example.com',
        displayName: 'テストユーザー',
        storeCode: '001',
        storeName: '本店',
        roles: ['User'],
        isActive: true,
        mustChangePassword: false,
      },
      isLoading: false,
      isError: false,
      login: vi.fn(),
      testLogin: vi.fn(),
      entraLogin: vi.fn(),
      logout: vi.fn(),
      changePassword: vi.fn(),
      resetPassword: vi.fn(),
      resetPasswordByCredentials: vi.fn(),
    })

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByTestId('home-page')).toBeInTheDocument()
    expect(screen.queryByTestId('login-page')).not.toBeInTheDocument()
  })

  it('パスワード変更必須の場合は変更ページを表示する', () => {
    mockUseAuth.mockReturnValue({
      user: {
        userId: 'user-1',
        email: 'test@example.com',
        displayName: 'テストユーザー',
        storeCode: '001',
        storeName: '本店',
        roles: ['general'],
        isActive: true,
        mustChangePassword: true,
      },
      isLoading: false,
      isError: false,
      login: vi.fn(),
      testLogin: vi.fn(),
      entraLogin: vi.fn(),
      logout: vi.fn(),
      changePassword: vi.fn(),
      resetPassword: vi.fn(),
      resetPasswordByCredentials: vi.fn(),
    })

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByTestId('change-password-page')).toBeInTheDocument()
  })
})
