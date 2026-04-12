import useAspidaSWR from '@aspida/swr'
import api from '@/api/$api'
import type { PasswordResetResultDto, UserDto } from '@/api/@types'
import { aspidaClient, aspidaClientNoThrow } from '@/lib/aspida'
import { asApiResponse } from '@/lib/apiResponse'

export type { PasswordResetResultDto, TestLoginUserDto, UserDto } from '@/api/@types'

const authApi = api(aspidaClient).api.Auth
const authApiNoThrow = api(aspidaClientNoThrow).api.Auth

/**
 * 認証状態と認証操作を提供するカスタムフック。
 * ログイン・ログアウト・パスワード変更などの認証 API を呼び出し、
 * 現在のユーザー情報を SWR でキャッシュ管理する。
 *
 * @returns 認証状態とアクション関数を含むオブジェクト
 * - `user` - ログイン中のユーザー情報。未ログイン時は `undefined`
 * - `isLoading` - 認証情報の取得中かどうか
 * - `isError` - 認証情報の取得にエラーが発生したかどうか
 * - `login` - メールアドレスとパスワードでログインする
 * - `testLogin` - テスト用ユーザー ID でログインする（開発環境専用）
 * - `entraLogin` - Azure Entra ID の ID トークンでログインする
 * - `logout` - ログアウトする
 * - `changePassword` - パスワードを変更する
 * - `resetPassword` - パスワードを初期化する（ログイン中ユーザー対象）
 * - `resetPasswordByCredentials` - メールアドレスと現在のパスワードでパスワードを初期化する
 */
export function useAuth() {
  const { data, error, isLoading, mutate } = useAspidaSWR(
    authApi.me,
    {
      revalidateOnFocus: false,
      shouldRetryOnError: false,
    },
  )

  /**
   * メールアドレスとパスワードでログインする。
   * 成功時は認証情報を再取得する。
   * @param email - メールアドレス
   * @param password - パスワード
   * @returns API レスポンス
   */
  const login = async (email: string, password: string) => {
    const response = asApiResponse<UserDto>(await authApiNoThrow.login.$post({
      body: { email, password },
    }))
    if (response.success) {
      await mutate()
    }
    return response
  }

  /**
   * テスト用ユーザー ID でログインする（開発環境専用）。
   * 成功時は認証情報を再取得する。
   * @param userId - テストログインするユーザー ID
   * @returns API レスポンス
   */
  const testLogin = async (userId: string) => {
    const response = asApiResponse<UserDto>(await authApiNoThrow.test_login.$post({
      body: { userId },
    }))
    if (response.success) {
      await mutate()
    }
    return response
  }

  /**
   * Azure Entra ID の ID トークンでログインする。
   * 成功時は認証情報を再取得する。
   * @param idToken - Entra ID から取得した ID トークン
   * @returns API レスポンス
   */
  const entraLogin = async (idToken: string) => {
    const response = asApiResponse<UserDto>(await authApiNoThrow.entra_login.$post({
      body: { idToken },
    }))
    if (response.success) {
      await mutate()
    }
    return response
  }

  /**
   * ログアウトする。
   * サーバー側のセッションを破棄し、認証情報キャッシュをクリアする。
   */
  const logout = async () => {
    await authApi.logout.$post()
    await mutate(undefined)
  }

  /**
   * ログイン中ユーザーのパスワードを変更する。
   * 成功時は認証情報を再取得する。
   * @param newPassword - 新しいパスワード
   * @returns API レスポンス
   */
  const changePassword = async (newPassword: string) => {
    const response = asApiResponse<UserDto>(await authApiNoThrow.change_password.$post({
      body: { newPassword },
    }))
    if (response.success) {
      await mutate()
    }
    return response
  }

  /**
   * ログイン中ユーザー自身のパスワードを初期化する。
   * @returns 初期パスワードを含む API レスポンス
   */
  const resetPassword = async () => {
    return asApiResponse<PasswordResetResultDto>(await authApiNoThrow.reset_password.$post())
  }

  /**
   * メールアドレスと現在のパスワードを使ってパスワードを初期化する。
   * ログイン画面から実行するパスワード初期化フローで使用する。
   * @param email - メールアドレス
   * @param currentPassword - 現在のパスワード
   * @returns 初期パスワードを含む API レスポンス
   */
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
