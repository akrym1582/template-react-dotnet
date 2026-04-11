import useSWR from 'swr'

const fetcher = async (url: string) => {
  const res = await fetch(url, { credentials: 'same-origin' })
  if (!res.ok) {
    throw new Error(`HTTP ${res.status}`)
  }
  return res.json()
}

export function useApi<T>(path: string | null) {
  return useSWR<T>(path, fetcher)
}
