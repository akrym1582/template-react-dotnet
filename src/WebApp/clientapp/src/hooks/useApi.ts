import useAspidaSWR from '@aspida/swr'
import { createJsonGetApi } from '@/lib/aspida'

/** `useApi` が無効化されているときのプレースホルダーパス */
const DISABLED_PATH_PLACEHOLDER = '/__useApi_disabled__'

/**
 * 指定したパスへ GET リクエストを行い、レスポンスデータを SWR でキャッシュ管理するフック。
 * `path` に `null` を渡すとフェッチを無効化できる。
 * @template T - レスポンスデータの型
 * @param path - フェッチ対象の API パス。`null` を指定するとフェッチを無効化する
 * @returns SWR の戻り値（`data`・`error`・`isLoading` など）
 */
export function useApi<T>(path: string | null) {
  const api = createJsonGetApi<T>(path ?? DISABLED_PATH_PLACEHOLDER)
  const option = path === null ? { key: null } : undefined

  return useAspidaSWR(api, option)
}
