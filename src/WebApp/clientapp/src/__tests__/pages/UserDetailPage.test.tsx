import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import UserDetailPage from '@/pages/UserDetailPage'

vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}))

vi.mock('@/lib/alert', () => ({
  alert: {
    error: vi.fn(),
    success: vi.fn(),
    confirm: vi.fn(),
  },
}))

import { useAuth } from '@/hooks/useAuth'

const mockUseAuth = vi.mocked(useAuth)
const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

beforeEach(() => {
  vi.clearAllMocks()
  mockUseAuth.mockReturnValue({
    user: {
      userId: 'user-1',
      email: 'user1@example.com',
      displayName: '本店ユーザー',
      storeCode: '001',
      storeName: '本店',
      roles: ['manager'],
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

  mockFetch.mockResolvedValue({
    json: vi.fn().mockResolvedValue({
      success: true,
      data: {
        userId: 'user-1',
        email: 'user1@example.com',
        displayName: '本店ユーザー',
        storeCode: '001',
        storeName: '本店',
        roles: ['manager'],
        isActive: true,
        mustChangePassword: false,
      },
    }),
  } as never)
})

describe('UserDetailPage', () => {
  it('ユーザーGUIDと自分のパスワード初期化ボタンを表示する', async () => {
    render(
      <MemoryRouter initialEntries={['/users/user-1']}>
        <Routes>
          <Route path="/users/:userId" element={<UserDetailPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText(/user-1/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: '自分のパスワードを初期化' })).toBeInTheDocument()
  })
})
