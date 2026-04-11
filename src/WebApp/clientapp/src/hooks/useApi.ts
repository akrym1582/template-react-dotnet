import useAspidaSWR from '@aspida/swr'
import { createJsonGetApi } from '@/lib/aspida'

const DISABLED_PATH_PLACEHOLDER = '/__useApi_disabled__'

export function useApi<T>(path: string | null) {
  const api = createJsonGetApi<T>(path ?? DISABLED_PATH_PLACEHOLDER)
  const option = path === null ? { key: null } : undefined

  return useAspidaSWR(api, option)
}
