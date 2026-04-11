import useAspidaSWR from '@aspida/swr'
import { createJsonGetApi } from '@/lib/aspida'

export function useApi<T>(path: string | null) {
  return useAspidaSWR(createJsonGetApi<T>(path ?? ''), { enabled: path !== null })
}
