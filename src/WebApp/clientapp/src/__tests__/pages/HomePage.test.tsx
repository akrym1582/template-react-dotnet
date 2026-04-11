import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import HomePage from '@/pages/HomePage'

vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}))

vi.mock('@/lib/alert', () => ({
  alert: {
    confirm: vi.fn(),
    success: vi.fn(),
  },
}))

import { useAuth } from '@/hooks/useAuth'
import { alert } from '@/lib/alert'

const mockUseAuth = vi.mocked(useAuth)
const mockAlert = vi.mocked(alert)

const mockLogout = vi.fn()
const mockUser = {
  userId: 'user-1',
  email: 'test@example.com',
  displayName: 'テストユーザー',
  roles: ['Admin', 'User'],
  isActive: true,
}

beforeEach(() => {
  vi.clearAllMocks()
  mockUseAuth.mockReturnValue({
    user: mockUser,
    isLoading: false,
    isError: false,
    login: vi.fn(),
    testLogin: vi.fn(),
    entraLogin: vi.fn(),
    logout: mockLogout,
  })
})

describe('HomePage', () => {
  it('ダッシュボードタイトルを表示する', () => {
    render(<HomePage />)
    expect(screen.getByText('ダッシュボード')).toBeInTheDocument()
  })

  it('ユーザーの表示名を表示する', () => {
    render(<HomePage />)
    expect(screen.getByText('テストユーザー')).toBeInTheDocument()
  })

  it('ユーザーのメールアドレスを表示する', () => {
    render(<HomePage />)
    expect(screen.getByText('test@example.com')).toBeInTheDocument()
  })

  it('ユーザーのロールを表示する', () => {
    render(<HomePage />)
    expect(screen.getByText('Admin, User')).toBeInTheDocument()
  })

  it('ログアウトボタンを表示する', () => {
    render(<HomePage />)
    expect(screen.getByRole('button', { name: 'ログアウト' })).toBeInTheDocument()
  })

  it('ログアウト確認後に logout を呼び出す', async () => {
    mockAlert.confirm.mockResolvedValue(true)
    mockAlert.success.mockResolvedValue(undefined as never)
    mockLogout.mockResolvedValue(undefined)

    render(<HomePage />)

    await userEvent.click(screen.getByRole('button', { name: 'ログアウト' }))

    await waitFor(() => {
      expect(mockAlert.confirm).toHaveBeenCalledWith('ログアウトしますか？')
      expect(mockLogout).toHaveBeenCalled()
      expect(mockAlert.success).toHaveBeenCalledWith('ログアウトしました。')
    })
  })

  it('ログアウトをキャンセルした場合は logout を呼び出さない', async () => {
    mockAlert.confirm.mockResolvedValue(false)

    render(<HomePage />)

    await userEvent.click(screen.getByRole('button', { name: 'ログアウト' }))

    await waitFor(() => {
      expect(mockAlert.confirm).toHaveBeenCalled()
    })
    expect(mockLogout).not.toHaveBeenCalled()
  })
})
