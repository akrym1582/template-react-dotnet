import { render, screen } from '@testing-library/react'
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

import { useAuth } from '@/hooks/useAuth'

const mockUseAuth = vi.mocked(useAuth)

describe('App', () => {
  it('ローディング中はローディング表示を返す', () => {
    mockUseAuth.mockReturnValue({
      user: undefined,
      isLoading: true,
      isError: false,
      login: vi.fn(),
      entraLogin: vi.fn(),
      logout: vi.fn(),
    })

    render(<App />)

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
      entraLogin: vi.fn(),
      logout: vi.fn(),
    })

    render(<App />)

    expect(screen.getByTestId('login-page')).toBeInTheDocument()
    expect(screen.queryByTestId('home-page')).not.toBeInTheDocument()
  })

  it('認証済みの場合はホームページを表示する', () => {
    mockUseAuth.mockReturnValue({
      user: {
        userId: 'user-1',
        email: 'test@example.com',
        displayName: 'テストユーザー',
        roles: ['User'],
        isActive: true,
      },
      isLoading: false,
      isError: false,
      login: vi.fn(),
      entraLogin: vi.fn(),
      logout: vi.fn(),
    })

    render(<App />)

    expect(screen.getByTestId('home-page')).toBeInTheDocument()
    expect(screen.queryByTestId('login-page')).not.toBeInTheDocument()
  })
})
