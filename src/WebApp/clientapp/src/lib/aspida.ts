import aspida from '@aspida/fetch'

const XSRF_COOKIE_NAME = 'XSRF-TOKEN'
const XSRF_HEADER_NAME = 'X-XSRF-TOKEN'

const fetchConfig = {
  baseURL: '/api',
  credentials: 'same-origin' as const,
  throwHttpErrors: true,
}

const getXsrfToken = () =>
  document.cookie
    .split(';')
    .map((cookie) => cookie.trim())
    .find((cookie) => cookie.startsWith(`${XSRF_COOKIE_NAME}=`))
    ?.slice(`${XSRF_COOKIE_NAME}=`.length)

const isApiRequest = (input: RequestInfo | URL) => {
  const requestUrl =
    typeof input === 'string'
      ? input
      : input instanceof URL
        ? input.toString()
        : input.url

  return requestUrl.startsWith('/api') || requestUrl.startsWith(`${window.location.origin}/api`)
}

export const apiFetch: typeof fetch = (input, init) => {
  const requestInit: RequestInit = {
    credentials: 'same-origin',
    ...init,
  }
  const xsrfToken = getXsrfToken()

  if (xsrfToken && isApiRequest(input)) {
    const headers = new Headers(init?.headers)
    headers.set(XSRF_HEADER_NAME, xsrfToken)
    requestInit.headers = headers
  }

  return fetch(input, requestInit)
}

export const aspidaClient = aspida(apiFetch, fetchConfig)
export const aspidaClientNoThrowHttpErrors = aspida(apiFetch, {
  ...fetchConfig,
  throwHttpErrors: false,
})

type JsonGetApi<T> = {
  $get: (_option?: object) => Promise<T>
  $path: (_option?: object) => string
}

export const createJsonGetApi = <T>(path: string): JsonGetApi<T> => {
  return {
    $get: async () => {
      const res = await apiFetch(path)
      if (!res.ok) {
        throw new Error(`エラー: ${path} の取得に失敗しました (HTTP ${res.status} ${res.statusText})`)
      }
      return (await res.json()) as T
    },
    $path: () => path,
  }
}
