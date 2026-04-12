import useAspidaSWR from '@aspida/swr'
import api from '@/api/$api'
import { aspidaClient, aspidaClientNoThrowHttpErrors } from '@/lib/aspida'

export type { PasswordResetResultDto, TestLoginUserDto, UserDto } from '@/api/auth/_types'

const authApi = api(aspidaClient).auth
const authApiNoThrow = api(aspidaClientNoThrowHttpErrors).auth

export function useAuth() {
  const { data, error, isLoading, mutate } = useAspidaSWR(
    authApi.me,
    {
      revalidateOnFocus: false,
      shouldRetryOnError: false,
    },
  )

  const login = async (email: string, password: string) => {
    const json = await authApiNoThrow.login.$post({
      body: { email, password },
    })
    if (json.success) {
      await mutate()
    }
    return json
  }

  const testLogin = async (userId: string) => {
    const json = await authApiNoThrow.test_login.$post({
      body: { userId },
    })
    if (json.success) {
      await mutate()
    }
    return json
  }

  const entraLogin = async (idToken: string) => {
    const json = await authApiNoThrow.entra_login.$post({
      body: { idToken },
    })
    if (json.success) {
      await mutate()
    }
    return json
  }

  const logout = async () => {
    await authApi.logout.$post()
    await mutate(undefined)
  }

  const changePassword = async (newPassword: string) => {
    const json = await authApiNoThrow.change_password.$post({
      body: { newPassword },
    })
    if (json.success) {
      await mutate()
    }
    return json
  }

  const resetPassword = async () => {
    return authApiNoThrow.reset_password.$post()
  }

  const resetPasswordByCredentials = async (email: string, currentPassword: string) => {
    return authApiNoThrow.reset_password_by_credentials.$post({
      body: { email, currentPassword },
    })
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
