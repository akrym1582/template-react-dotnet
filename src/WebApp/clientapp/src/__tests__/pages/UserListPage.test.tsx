import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import UserListPage from '@/pages/UserListPage'

vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}))

vi.mock('@/lib/alert', () => ({
  alert: {
    error: vi.fn(),
    success: vi.fn(),
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
      userId: 'manager-1',
      email: 'manager@example.com',
      displayName: '役席ユーザー',
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
        allowUserCreation: true,
        users: [
          {
            userId: 'user-1',
            email: 'user1@example.com',
            displayName: '本店ユーザー',
            storeCode: '001',
            storeName: '本店',
            roles: ['general'],
            isActive: true,
            mustChangePassword: false,
          },
          {
            userId: 'user-2',
            email: 'user2@example.com',
            displayName: '支店ユーザー',
            storeCode: '002',
            storeName: '支店',
            roles: ['general'],
            isActive: true,
            mustChangePassword: false,
          },
        ],
      },
    }),
  } as never)
})

describe('UserListPage', () => {
  it('最初の店番で一覧を表示する', async () => {
    render(
      <MemoryRouter>
        <UserListPage />
      </MemoryRouter>,
    )

    expect(await screen.findByText('本店ユーザー')).toBeInTheDocument()
    expect(screen.queryByText('支店ユーザー')).not.toBeInTheDocument()
  })
})
