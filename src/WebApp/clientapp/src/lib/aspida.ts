import aspida from '@aspida/fetch'

/** XSRF トークンが格納される Cookie 名 */
const XSRF_COOKIE_NAME = 'XSRF-TOKEN'
/** XSRF トークンを送信するリクエストヘッダー名 */
const XSRF_HEADER_NAME = 'X-XSRF-TOKEN'

const fetchConfig = {
  baseURL: '',
  credentials: 'same-origin' as const,
  throwHttpErrors: true,
}

/**
 * Cookie から XSRF トークンを取得する。
 * @returns XSRF トークン文字列。Cookie が存在しない場合は `undefined`
 */
const getXsrfToken = () =>
  document.cookie
    .split(';')
    .map((cookie) => cookie.trim())
    .find((cookie) => cookie.startsWith(`${XSRF_COOKIE_NAME}=`))
    ?.slice(`${XSRF_COOKIE_NAME}=`.length)

/**
 * リクエスト URL が API エンドポイント（`/api` 配下）かどうかを判定する。
 * @param input - 判定するリクエスト情報
 * @returns API エンドポイントの場合は `true`
 */
const isApiRequest = (input: RequestInfo | URL) => {
  const requestUrl =
    typeof input === 'string'
      ? input
      : input instanceof URL
        ? input.toString()
        : input.url

  return requestUrl.startsWith('/api') || requestUrl.startsWith(`${window.location.origin}/api`)
}

/**
 * XSRF トークンを自動付与するカスタム fetch 関数。
 * API リクエストに対してのみ `X-XSRF-TOKEN` ヘッダーを付与する。
 * @param input - リクエスト URL またはリクエスト情報
 * @param init - fetch の初期化オプション
 * @returns fetch のレスポンス Promise
 */
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

/** XSRF トークン付与・HTTP エラー時例外送出を行う aspida クライアント */
export const aspidaClient = aspida(apiFetch, fetchConfig)
/** XSRF トークン付与を行うが HTTP エラー時は例外を送出しない aspida クライアント */
export const aspidaClientNoThrow = aspida(apiFetch, {
  ...fetchConfig,
  throwHttpErrors: false,
})

/** `useApi` フック内で使用する JSON GET API の最小インターフェース */
type JsonGetApi<T> = {
  $get: (_option?: object) => Promise<T>
  $path: (_option?: object) => string
}

/**
 * 指定したパスへ GET リクエストを行う JSON 取得 API オブジェクトを生成する。
 * `useApi` フックなど aspida 非対応エンドポイントへのアクセスに使用する。
 * @param path - API のパス（例: `/api/user`）
 * @returns `$get` と `$path` を持つ API オブジェクト
 */
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
