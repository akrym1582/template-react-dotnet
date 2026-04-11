import api from '@/api/$api'
import { aspidaClient } from '@/lib/aspida'
import useSWR from 'swr'

export interface UserDto {
  userId: string
  email: string
  displayName: string
  roles: string[]
  isActive: boolean
}

export interface TestLoginUserDto {
  userId: string
  roles: string[]
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

const authApi = api(aspidaClient).auth

export function useAuth() {
  const { data, error, isLoading, mutate } = useSWR<ApiResponse<UserDto>>(
    authApi.me.$path(),
    fetcher,
    {
      revalidateOnFocus: false,
      shouldRetryOnError: false,
    },
  )

  const login = async (email: string, password: string) => {
    const res = await fetch(authApi.login.$path(), {
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

  const testLogin = async (userId: string) => {
    const res = await fetch('/api/auth/test-login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'same-origin',
      body: JSON.stringify({ userId }),
    })
    const json: ApiResponse<UserDto> = await res.json()
    if (json.success) {
      await mutate()
    }
    return json
  }

  const entraLogin = async (idToken: string) => {
    const res = await fetch(authApi.entra_login.$path(), {
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
    await fetch(authApi.logout.$path(), {
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
    testLogin,
    entraLogin,
    logout,
  }
}
