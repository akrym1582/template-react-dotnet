import useSWR from 'swr'

export interface UserDto {
  userId: string
  email: string
  displayName: string
  roles: string[]
  isActive: boolean
}

interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
}

const fetcher = async (url: string) => {
  const res = await fetch(url, { credentials: 'same-origin' })
  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  return res.json()
}

export function useAuth() {
  const { data, error, isLoading, mutate } = useSWR<ApiResponse<UserDto>>(
    '/api/auth/me',
    fetcher,
    {
      revalidateOnFocus: false,
      shouldRetryOnError: false,
    },
  )

  const login = async (email: string, password: string) => {
    const res = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'same-origin',
      body: JSON.stringify({ email, password }),
    })
    const json: ApiResponse<UserDto> = await res.json()
    if (json.success) {
      await mutate()
    }
    return json
  }

  const entraLogin = async (idToken: string) => {
    const res = await fetch('/api/auth/entra-login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'same-origin',
      body: JSON.stringify({ idToken }),
    })
    const json: ApiResponse<UserDto> = await res.json()
    if (json.success) {
      await mutate()
    }
    return json
  }

  const logout = async () => {
    await fetch('/api/auth/logout', {
      method: 'POST',
      credentials: 'same-origin',
    })
    await mutate(undefined)
  }

  return {
    user: data?.success ? data.data : undefined,
    isLoading,
    isError: !!error,
    login,
    entraLogin,
    logout,
  }
}
