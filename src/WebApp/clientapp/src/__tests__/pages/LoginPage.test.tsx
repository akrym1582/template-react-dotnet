import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import LoginPage from '@/pages/LoginPage'

vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}))

vi.mock('@/lib/alert', () => ({
  alert: {
    withLoading: vi.fn(),
    error: vi.fn(),
    success: vi.fn(),
  },
}))

import { useAuth } from '@/hooks/useAuth'
import { alert } from '@/lib/alert'

const mockUseAuth = vi.mocked(useAuth)
const mockAlert = vi.mocked(alert)
const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

const mockLogin = vi.fn()
const mockTestLogin = vi.fn()

beforeEach(() => {
  vi.clearAllMocks()
  mockFetch.mockResolvedValue({
    ok: true,
    json: vi.fn().mockResolvedValue({
      success: true,
      data: [{ userId: 'test-user', roles: ['user'] }],
    }),
  } as never)
  mockUseAuth.mockReturnValue({
    user: undefined,
    isLoading: false,
    isError: false,
    login: mockLogin,
    testLogin: mockTestLogin,
    entraLogin: vi.fn(),
    logout: vi.fn(),
    changePassword: vi.fn(),
    resetPassword: vi.fn(),
    resetPasswordByCredentials: vi.fn(),
  })
})

describe('LoginPage', () => {
  it('ログインフォームを表示する', async () => {
    render(<LoginPage />)
    await screen.findByRole('button', { name: /test-user/i })

    expect(screen.getByRole('heading', { name: 'ログイン' })).toBeInTheDocument()
    expect(screen.getByLabelText('メールアドレス')).toBeInTheDocument()
    expect(screen.getByLabelText('パスワード')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'ログイン' })).toBeInTheDocument()
  })

  it('メールアドレスとパスワードを入力できる', async () => {
    render(<LoginPage />)

    const emailInput = screen.getByLabelText('メールアドレス')
    const passwordInput = screen.getByLabelText('パスワード')

    await userEvent.type(emailInput, 'test@example.com')
    await userEvent.type(passwordInput, 'password123')

    expect(emailInput).toHaveValue('test@example.com')
    expect(passwordInput).toHaveValue('password123')
  })

  it('フォーム送信時に login を呼び出す', async () => {
    mockAlert.withLoading.mockImplementation(async (action) => action())
    mockLogin.mockResolvedValue({ success: true })

    render(<LoginPage />)

    await userEvent.type(screen.getByLabelText('メールアドレス'), 'test@example.com')
    await userEvent.type(screen.getByLabelText('パスワード'), 'password123')
    await userEvent.click(screen.getByRole('button', { name: 'ログイン' }))

    await waitFor(() => {
      expect(mockAlert.withLoading).toHaveBeenCalled()
      expect(mockLogin).toHaveBeenCalledWith('test@example.com', 'password123')
    })
  })

  it('テストログインユーザーを表示する', async () => {
    render(<LoginPage />)

    expect(await screen.findByRole('button', { name: /test-user/i })).toBeInTheDocument()
    expect(screen.getByText('user')).toBeInTheDocument()
  })

  it('テストログインボタン押下時に testLogin を呼び出す', async () => {
    mockAlert.withLoading.mockImplementation(async (action) => action())
    mockTestLogin.mockResolvedValue({ success: true })

    render(<LoginPage />)

    await userEvent.click(await screen.findByRole('button', { name: /test-user/i }))

    await waitFor(() => {
      expect(mockAlert.withLoading).toHaveBeenCalled()
      expect(mockTestLogin).toHaveBeenCalledWith('test-user')
    })
  })

  it('ログイン失敗時に error アラートを表示する', async () => {
    const errorMessage = 'メールアドレスまたはパスワードが違います。'
    mockAlert.withLoading.mockImplementation(async (action) => action())
    mockAlert.error.mockResolvedValue(undefined as never)
    mockLogin.mockResolvedValue({ success: false, message: errorMessage })

    render(<LoginPage />)

    await userEvent.type(screen.getByLabelText('メールアドレス'), 'test@example.com')
    await userEvent.type(screen.getByLabelText('パスワード'), 'wrong')
    await userEvent.click(screen.getByRole('button', { name: 'ログイン' }))

    await waitFor(() => {
      expect(mockAlert.error).toHaveBeenCalledWith(errorMessage)
    })
  })

  it('送信中はボタンが無効になる', async () => {
    let resolveLogin!: (value: unknown) => void
    const pendingLogin = new Promise((resolve) => {
      resolveLogin = resolve
    })
    mockAlert.withLoading.mockReturnValue(pendingLogin as never)

    render(<LoginPage />)

    await userEvent.type(screen.getByLabelText('メールアドレス'), 'test@example.com')
    await userEvent.type(screen.getByLabelText('パスワード'), 'password')
    await userEvent.click(screen.getByRole('button', { name: 'ログイン' }))

    expect(screen.getByRole('button', { name: 'ログイン中...' })).toBeDisabled()

    resolveLogin({ success: true })
    await waitFor(() => expect(screen.getByRole('button', { name: 'ログイン' })).not.toBeDisabled())
  })

  it('Azure Entra ID ボタンが表示される', async () => {
    render(<LoginPage />)
    await screen.findByRole('button', { name: /test-user/i })
    expect(screen.getByRole('button', { name: 'Azure Entra ID でログイン' })).toBeInTheDocument()
  })

  it('パスワード初期化ボタンが表示される', async () => {
    render(<LoginPage />)
    await screen.findByRole('button', { name: /test-user/i })
    expect(screen.getByRole('button', { name: '現在のパスワードで初期化' })).toBeInTheDocument()
  })
})
