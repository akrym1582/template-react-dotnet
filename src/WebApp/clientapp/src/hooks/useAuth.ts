import useAspidaSWR from '@aspida/swr'
import api from '@/api/$api'
import type { PasswordResetResultDto, UserDto } from '@/api/@types'
import { aspidaClient, aspidaClientNoThrow } from '@/lib/aspida'

export type { PasswordResetResultDto, TestLoginUserDto, UserDto } from '@/api/@types'

type ApiResponse<T> = {
  success: boolean
  data?: T | null
  message?: string | null
}

const authApi = api(aspidaClient).api.Auth
const authApiNoThrow = api(aspidaClientNoThrow).api.Auth

export function useAuth() {
  const { data, error, isLoading, mutate } = useAspidaSWR(
    authApi.me,
    {
      revalidateOnFocus: false,
      shouldRetryOnError: false,
    },
  )

  const login = async (email: string, password: string) => {
    const response = (await authApiNoThrow.login.$post({
      body: { email, password },
    })) as ApiResponse<UserDto>
    if (response.success) {
      await mutate()
    }
    return response
  }

  const testLogin = async (userId: string) => {
    const response = (await authApiNoThrow.test_login.$post({
      body: { userId },
    })) as ApiResponse<UserDto>
    if (response.success) {
      await mutate()
    }
    return response
  }

  const entraLogin = async (idToken: string) => {
    const response = (await authApiNoThrow.entra_login.$post({
      body: { idToken },
    })) as ApiResponse<UserDto>
    if (response.success) {
      await mutate()
    }
    return response
  }

  const logout = async () => {
    await authApi.logout.$post()
    await mutate(undefined)
  }

  const changePassword = async (newPassword: string) => {
    const response = (await authApiNoThrow.change_password.$post({
      body: { newPassword },
    })) as ApiResponse<UserDto>
    if (response.success) {
      await mutate()
    }
    return response
  }

  const resetPassword = async () => {
    return (await authApiNoThrow.reset_password.$post()) as ApiResponse<PasswordResetResultDto>
  }

  const resetPasswordByCredentials = async (email: string, currentPassword: string) => {
    return (await authApiNoThrow.reset_password_by_credentials.$post({
      body: { email, currentPassword },
    })) as ApiResponse<PasswordResetResultDto>
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
