import api from '@/api/$api'
import { aspidaClient } from '@/lib/aspida'
import useSWR from 'swr'

export interface UserDto {
  userId: string
  email: string
  displayName: string
  storeCode: string
  storeName: string
  roles: string[]
  isActive: boolean
  mustChangePassword: boolean
}

export interface TestLoginUserDto {
  userId: string
  roles: string[]
}

export interface PasswordResetResultDto {
  initialPassword: string
  mustChangePassword: boolean
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

  const changePassword = async (newPassword: string) => {
    const res = await fetch('/api/auth/change-password', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'same-origin',
      body: JSON.stringify({ newPassword }),
    })
    const json: ApiResponse<UserDto> = await res.json()
    if (json.success) {
      await mutate()
    }
    return json
  }

  const resetPassword = async () => {
    const res = await fetch('/api/auth/reset-password', {
      method: 'POST',
      credentials: 'same-origin',
    })
    return (await res.json()) as ApiResponse<PasswordResetResultDto>
  }

  const resetPasswordByCredentials = async (email: string, currentPassword: string) => {
    const res = await fetch('/api/auth/reset-password-by-credentials', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'same-origin',
      body: JSON.stringify({ email, currentPassword }),
    })
    return (await res.json()) as ApiResponse<PasswordResetResultDto>
  }

  return {
    user: data?.success ? data.data : undefined,
    isLoading,
    isError: !!error,
    login,
    testLogin,
    entraLogin,
    logout,
    changePassword,
    resetPassword,
    resetPasswordByCredentials,
  }
}
