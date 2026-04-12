import { apiFetch } from '@/lib/aspida'

const mockFetch = vi.fn()
vi.stubGlobal('fetch', mockFetch)

afterEach(() => {
  document.cookie = 'XSRF-TOKEN=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/'
  vi.clearAllMocks()
})

describe('apiFetch', () => {
  it('XSRF cookie がある場合は API リクエストにヘッダーを付与する', async () => {
    document.cookie = 'XSRF-TOKEN=test-token'
    mockFetch.mockResolvedValue(new Response(null, { status: 200 }))

    await apiFetch('/api/user', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    })

    expect(mockFetch).toHaveBeenCalledWith(
      '/api/user',
      expect.objectContaining({
        credentials: 'same-origin',
        headers: expect.any(Headers),
      }),
    )

    const [, init] = mockFetch.mock.calls[0]
    const headers = init.headers as Headers
    expect(headers.get('Content-Type')).toBe('application/json')
    expect(headers.get('X-XSRF-TOKEN')).toBe('test-token')
  })

  it('XSRF cookie が無い場合はヘッダーを追加しない', async () => {
    mockFetch.mockResolvedValue(new Response(null, { status: 200 }))

    await apiFetch('/api/user')

    expect(mockFetch).toHaveBeenCalledWith('/api/user', { credentials: 'same-origin' })
  })
})
