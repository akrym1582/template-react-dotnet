import useAspidaSWR from '@aspida/swr'
import { createJsonGetApi } from '@/lib/aspida'

const disabledApiPath = '/__useApi_disabled__'

export function useApi<T>(path: string | null) {
  const api = createJsonGetApi<T>(path ?? disabledApiPath)
  const option = path === null ? { key: null } : undefined

  return useAspidaSWR(api, option)
}
