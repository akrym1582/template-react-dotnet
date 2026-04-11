import aspida from '@aspida/fetch'

const fetchConfig = {
  baseURL: '/api',
  credentials: 'same-origin' as const,
  throwHttpErrors: true,
}

export const aspidaClient = aspida(fetch, fetchConfig)

type JsonGetApi<T> = {
  $get: (_option?: object) => Promise<T>
  $path: (_option?: object) => string
}

export const createJsonGetApi = <T>(path: string): JsonGetApi<T> => {
  return {
    $get: async () => {
      const res = await fetch(path, { credentials: 'same-origin' })
      if (!res.ok) {
        throw new Error(`エラー: ${path} の取得に失敗しました (HTTP ${res.status} ${res.statusText})`)
      }
      return (await res.json()) as T
    },
    $path: () => path,
  }
}
