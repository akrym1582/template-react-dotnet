import useAspidaSWR from '@aspida/swr'
import api from '@/api/$api'
import type { PasswordResetResultDto, UserDto } from '@/api/@types'
import { aspidaClient, aspidaClientNoThrow } from '@/lib/aspida'
import { asApiResponse } from '@/lib/apiResponse'

export type { PasswordResetResultDto, TestLoginUserDto, UserDto } from '@/api/@types'

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
    const response = asApiResponse<UserDto>(await authApiNoThrow.login.$post({
      body: { email, password },
    }))
    if (response.success) {
      await mutate()
    }
    return response
  }

  const testLogin = async (userId: string) => {
    const response = asApiResponse<UserDto>(await authApiNoThrow.test_login.$post({
      body: { userId },
    }))
    if (response.success) {
      await mutate()
    }
    return response
  }

  const entraLogin = async (idToken: string) => {
    const response = asApiResponse<UserDto>(await authApiNoThrow.entra_login.$post({
      body: { idToken },
    }))
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
    const response = asApiResponse<UserDto>(await authApiNoThrow.change_password.$post({
      body: { newPassword },
    }))
    if (response.success) {
      await mutate()
    }
    return response
  }

  const resetPassword = async () => {
    return asApiResponse<PasswordResetResultDto>(await authApiNoThrow.reset_password.$post())
  }

  const resetPasswordByCredentials = async (email: string, currentPassword: string) => {
    return asApiResponse<PasswordResetResultDto>(await authApiNoThrow.reset_password_by_credentials.$post({
      body: { email, currentPassword },
    }))
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
