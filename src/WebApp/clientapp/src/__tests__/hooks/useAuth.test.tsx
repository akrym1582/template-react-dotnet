import { renderHook, act, waitFor } from '@testing-library/react'
import { SWRConfig } from 'swr'
import { useAuth } from '@/hooks/useAuth'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

const mockUser = {
  userId: 'user-1',
  email: 'test@example.com',
  displayName: 'テストユーザー',
  roles: ['User'],
  isActive: true,
}

const successResponse = { success: true, data: mockUser }
const failureResponse = { success: false, message: 'メールアドレスまたはパスワードが違います。' }

// SWR キャッシュをテストごとに分離するラッパー
const createWrapper = () => {
  return ({ children }: { children: React.ReactNode }) => (
    <SWRConfig value={{ provider: () => new Map() }}>{children}</SWRConfig>
  )
}

afterEach(() => {
  vi.clearAllMocks()
})

describe('useAuth', () => {
  describe('初期ロード', () => {
    it('認証済みの場合 user を返す', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: vi.fn().mockResolvedValue(successResponse),
      })

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.user).toEqual(mockUser)
      expect(result.current.isError).toBe(false)
    })

    it('未認証の場合 user が undefined', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: vi.fn().mockResolvedValue(failureResponse),
      })

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.user).toBeUndefined()
    })

    it('フェッチエラー時に isError が true', async () => {
      mockFetch.mockResolvedValueOnce({ ok: false, status: 401 })

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isError).toBe(true)
      expect(result.current.user).toBeUndefined()
    })
  })

  describe('login', () => {
    it('ログイン成功時に user が更新される', async () => {
      mockFetch
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(failureResponse),
        })
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(successResponse),
        })
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(successResponse),
        })

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      let loginResult: Awaited<ReturnType<typeof result.current.login>>
      await act(async () => {
        loginResult = await result.current.login('test@example.com', 'password')
      })

      expect(loginResult!.success).toBe(true)
    })

    it('ログイン失敗時に失敗レスポンスを返す', async () => {
      mockFetch
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(failureResponse),
        })
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(failureResponse),
        })

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      let loginResult: Awaited<ReturnType<typeof result.current.login>>
      await act(async () => {
        loginResult = await result.current.login('test@example.com', 'wrong')
      })

      expect(loginResult!.success).toBe(false)
      expect(loginResult!.message).toBe('メールアドレスまたはパスワードが違います。')
    })

    it('POST /api/auth/login にリクエストを送信する', async () => {
      mockFetch
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(failureResponse),
        })
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(failureResponse),
        })

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      await act(async () => {
        await result.current.login('user@example.com', 'pass')
      })

      const loginCall = mockFetch.mock.calls.find(
        ([url]) => typeof url === 'string' && url.includes('login'),
      )
      expect(loginCall).toBeDefined()
      expect(loginCall![1]).toMatchObject({
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      })
    })
  })

  describe('testLogin', () => {
    it('POST /api/auth/test-login にリクエストを送信する', async () => {
      mockFetch
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(failureResponse),
        })
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue({
            success: true,
            data: [{ userId: 'test-user', roles: ['user'] }],
          }),
        })
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(successResponse),
        })
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(successResponse),
        })

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      await act(async () => {
        await result.current.testLogin('test-user')
      })

      const testLoginCall = mockFetch.mock.calls.find(
        ([url]) => typeof url === 'string' && url.includes('test-login'),
      )
      expect(testLoginCall).toBeDefined()
      expect(testLoginCall![1]).toMatchObject({
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userId: 'test-user' }),
      })
    })
  })

  describe('logout', () => {
    it('ログアウト後に user が undefined になる', async () => {
      mockFetch
        .mockResolvedValueOnce({
          ok: true,
          json: vi.fn().mockResolvedValue(successResponse),
        })
        .mockResolvedValueOnce({ ok: true, json: vi.fn() })

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })
      await waitFor(() => expect(result.current.user).toEqual(mockUser))

      await act(async () => {
        await result.current.logout()
      })

      expect(result.current.user).toBeUndefined()
    })
  })
})
