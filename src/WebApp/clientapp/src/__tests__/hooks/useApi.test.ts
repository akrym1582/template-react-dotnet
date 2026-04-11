import { renderHook, waitFor } from '@testing-library/react'
import { useApi } from '@/hooks/useApi'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

afterEach(() => {
  vi.clearAllMocks()
})

describe('useApi', () => {
  it('データを正常に取得する', async () => {
    const mockData = { id: 1, name: 'テスト' }
    mockFetch.mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue(mockData),
    })

    const { result } = renderHook(() => useApi<typeof mockData>('/api/test'))

    await waitFor(() => expect(result.current.data).toEqual(mockData))
    expect(mockFetch).toHaveBeenCalledWith('/api/test', { credentials: 'same-origin' })
  })

  it('HTTP エラー時に error がセットされる', async () => {
    mockFetch.mockResolvedValue({
      ok: false,
      status: 404,
      json: vi.fn(),
    })

    const { result } = renderHook(() => useApi<unknown>('/api/not-found'))

    await waitFor(() => expect(result.current.error).toBeDefined())
    expect(result.current.data).toBeUndefined()
  })

  it('path が null の場合はフェッチしない', async () => {
    const { result } = renderHook(() => useApi<unknown>(null))

    await waitFor(() => expect(result.current.isLoading).toBe(false))
    expect(mockFetch).not.toHaveBeenCalled()
    expect(result.current.data).toBeUndefined()
  })

  it('初期状態では isLoading が true', () => {
    mockFetch.mockReturnValue(new Promise(() => {}))

    const { result } = renderHook(() => useApi<unknown>('/api/slow'))

    expect(result.current.isLoading).toBe(true)
  })
})
