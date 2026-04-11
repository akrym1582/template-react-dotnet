import useAspidaSWR from '@aspida/swr'
import { createJsonGetApi } from '@/lib/aspida'

const DISABLED_API_PATH = '/__useApi_disabled__'

export function useApi<T>(path: string | null) {
  const api = createJsonGetApi<T>(path ?? DISABLED_API_PATH)
  const option = path === null ? { key: null } : undefined

  return useAspidaSWR(api, option)
}
