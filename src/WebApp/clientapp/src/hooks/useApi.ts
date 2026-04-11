import useAspidaSWR from '@aspida/swr'
import { createJsonGetApi } from '@/lib/aspida'

const useNullableAspidaSWR = <T,>(
  api: ReturnType<typeof createJsonGetApi<T>> | null,
  option: object,
) => {
  return useAspidaSWR(api as never, option as never)
}

export function useApi<T>(path: string | null) {
  return useNullableAspidaSWR(path === null ? null : createJsonGetApi<T>(path), {
    key: path === null ? null : undefined,
    fetcher: path === null ? null : undefined,
  })
}
