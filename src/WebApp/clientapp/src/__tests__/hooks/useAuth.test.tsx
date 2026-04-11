import { renderHook, act, waitFor } from '@testing-library/react'
import { SWRConfig } from 'swr'
import { useAuth } from '@/hooks/useAuth'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

const mockUser = {
  userId: 'user-1',
  email: 'test@example.com',
  displayName: 'テストユーザー',
  storeCode: '001',
  storeName: '本店',
  roles: ['User'],
  isActive: true,
  mustChangePassword: false,
}

const successResponse = { success: true, data: mockUser }
const failureResponse = { success: false, message: 'メールアドレスまたはパスワードが違います。' }

const createJsonResponse = <T,>(data: T, init?: { ok?: boolean; status?: number; statusText?: string }) => ({
  ok: init?.ok ?? true,
  status: init?.status ?? 200,
  statusText: init?.statusText ?? 'OK',
  headers: new Headers({ 'content-type': 'application/json' }),
  json: vi.fn().mockResolvedValue(data),
  text: vi.fn(),
  arrayBuffer: vi.fn(),
  blob: vi.fn(),
  formData: vi.fn(),
})

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
      mockFetch.mockResolvedValueOnce(createJsonResponse(successResponse))

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.user).toEqual(mockUser)
      expect(result.current.isError).toBe(false)
      expect(mockFetch).toHaveBeenCalledWith(
        '/api/auth/me',
        expect.objectContaining({ credentials: 'same-origin' }),
      )
    })

    it('未認証の場合 user が undefined', async () => {
      mockFetch.mockResolvedValueOnce(createJsonResponse(failureResponse))

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.user).toBeUndefined()
    })

    it('フェッチエラー時に isError が true', async () => {
      mockFetch.mockResolvedValueOnce(
        createJsonResponse(undefined, { ok: false, status: 401, statusText: 'Unauthorized' }),
      )

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })

      await waitFor(() => expect(result.current.isLoading).toBe(false))
      expect(result.current.isError).toBe(true)
      expect(result.current.user).toBeUndefined()
    })
  })

  describe('login', () => {
    it('ログイン成功時に user が更新される', async () => {
      mockFetch
        .mockResolvedValueOnce(createJsonResponse(failureResponse))
        .mockResolvedValueOnce(createJsonResponse(successResponse))
        .mockResolvedValueOnce(createJsonResponse(successResponse))

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
        .mockResolvedValueOnce(createJsonResponse(failureResponse))
        .mockResolvedValueOnce(createJsonResponse(failureResponse))

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
        .mockResolvedValueOnce(createJsonResponse(failureResponse))
        .mockResolvedValueOnce(createJsonResponse(failureResponse))

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
        credentials: 'same-origin',
      })
    })
  })

  describe('testLogin', () => {
    it('POST /api/auth/test-login にリクエストを送信する', async () => {
      mockFetch
        .mockResolvedValueOnce(createJsonResponse(failureResponse))
        .mockResolvedValueOnce(
          createJsonResponse({
            success: true,
            data: [{ userId: 'test-user', roles: ['user'] }],
          }),
        )
        .mockResolvedValueOnce(createJsonResponse(successResponse))
        .mockResolvedValueOnce(createJsonResponse(successResponse))

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
        credentials: 'same-origin',
        body: JSON.stringify({ userId: 'test-user' }),
      })
    })
  })

  describe('logout', () => {
    it('ログアウト後に user が undefined になる', async () => {
      mockFetch
        .mockResolvedValueOnce(createJsonResponse(successResponse))
        .mockResolvedValueOnce(createJsonResponse(undefined))

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })
      await waitFor(() => expect(result.current.user).toEqual(mockUser))

      await act(async () => {
        await result.current.logout()
      })

      expect(result.current.user).toBeUndefined()
    })
  })

  describe('changePassword', () => {
    it('POST /api/auth/change-password にリクエストを送信する', async () => {
      mockFetch
        .mockResolvedValueOnce(createJsonResponse(successResponse))
        .mockResolvedValueOnce(createJsonResponse(successResponse))
        .mockResolvedValueOnce(createJsonResponse(successResponse))

      const { result } = renderHook(() => useAuth(), { wrapper: createWrapper() })
      await waitFor(() => expect(result.current.isLoading).toBe(false))

      await act(async () => {
        await result.current.changePassword('NewPass@123')
      })

      const changePasswordCall = mockFetch.mock.calls.find(
        ([url]) => typeof url === 'string' && url.includes('change-password'),
      )
      expect(changePasswordCall).toBeDefined()
      expect(changePasswordCall![1]).toMatchObject({
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'same-origin',
        body: JSON.stringify({ newPassword: 'NewPass@123' }),
      })
    })
  })
})
