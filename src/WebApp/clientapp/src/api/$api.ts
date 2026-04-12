import type { AspidaClient, BasicHeaders } from 'aspida'
import type { Methods as MethodsChangePassword } from './auth/change-password'
import type { Methods as MethodsEntraLogin } from './auth/entra-login'
import type { Methods as MethodsLogin } from './auth/login'
import type { Methods as MethodsLogout } from './auth/logout'
import type { Methods as MethodsMe } from './auth/me'
import type { Methods as MethodsResetPassword } from './auth/reset-password'
import type { Methods as MethodsResetPasswordByCredentials } from './auth/reset-password-by-credentials'
import type { Methods as MethodsTestLogin } from './auth/test-login'
import type { Methods as MethodsTestUsers } from './auth/test-users'

const api = <T>({ baseURL, fetch }: AspidaClient<T>) => {
  const prefix = (baseURL === undefined ? '' : baseURL).replace(/\/$/, '')
  const PATH0 = '/auth/change-password'
  const PATH1 = '/auth/entra-login'
  const PATH2 = '/auth/login'
  const PATH3 = '/auth/logout'
  const PATH4 = '/auth/me'
  const PATH5 = '/auth/reset-password'
  const PATH6 = '/auth/reset-password-by-credentials'
  const PATH7 = '/auth/test-login'
  const PATH8 = '/auth/test-users'
  const GET = 'GET'
  const POST = 'POST'

  return {
    auth: {
      change_password: {
        post: (option: { body: MethodsChangePassword['post']['reqBody'], config?: T | undefined }) =>
          fetch<MethodsChangePassword['post']['resBody'], BasicHeaders, MethodsChangePassword['post']['status']>(
            prefix,
            PATH0,
            POST,
            option,
          ).json(),
        $post: (option: { body: MethodsChangePassword['post']['reqBody'], config?: T | undefined }) =>
          fetch<MethodsChangePassword['post']['resBody'], BasicHeaders, MethodsChangePassword['post']['status']>(
            prefix,
            PATH0,
            POST,
            option,
          )
            .json()
            .then(r => r.body),
        $path: () => `${prefix}${PATH0}`,
      },
      entra_login: {
        post: (option: { body: MethodsEntraLogin['post']['reqBody'], config?: T | undefined }) =>
          fetch<MethodsEntraLogin['post']['resBody'], BasicHeaders, MethodsEntraLogin['post']['status']>(
            prefix,
            PATH1,
            POST,
            option,
          ).json(),
        $post: (option: { body: MethodsEntraLogin['post']['reqBody'], config?: T | undefined }) =>
          fetch<MethodsEntraLogin['post']['resBody'], BasicHeaders, MethodsEntraLogin['post']['status']>(
            prefix,
            PATH1,
            POST,
            option,
          )
            .json()
            .then(r => r.body),
        $path: () => `${prefix}${PATH1}`,
      },
      login: {
        post: (option: { body: MethodsLogin['post']['reqBody'], config?: T | undefined }) =>
          fetch<MethodsLogin['post']['resBody'], BasicHeaders, MethodsLogin['post']['status']>(
            prefix,
            PATH2,
            POST,
            option,
          ).json(),
        $post: (option: { body: MethodsLogin['post']['reqBody'], config?: T | undefined }) =>
          fetch<MethodsLogin['post']['resBody'], BasicHeaders, MethodsLogin['post']['status']>(
            prefix,
            PATH2,
            POST,
            option,
          )
            .json()
            .then(r => r.body),
        $path: () => `${prefix}${PATH2}`,
      },
      logout: {
        post: (option?: { config?: T | undefined } | undefined) =>
          fetch<MethodsLogout['post']['resBody'], BasicHeaders, MethodsLogout['post']['status']>(
            prefix,
            PATH3,
            POST,
            option,
          ).json(),
        $post: (option?: { config?: T | undefined } | undefined) =>
          fetch<MethodsLogout['post']['resBody'], BasicHeaders, MethodsLogout['post']['status']>(
            prefix,
            PATH3,
            POST,
            option,
          )
            .json()
            .then(r => r.body),
        $path: () => `${prefix}${PATH3}`,
      },
      me: {
        get: (option?: { config?: T | undefined } | undefined) =>
          fetch<MethodsMe['get']['resBody'], BasicHeaders, MethodsMe['get']['status']>(prefix, PATH4, GET, option).json(),
        $get: (option?: { config?: T | undefined } | undefined) =>
          fetch<MethodsMe['get']['resBody'], BasicHeaders, MethodsMe['get']['status']>(prefix, PATH4, GET, option)
            .json()
            .then(r => r.body),
        $path: () => `${prefix}${PATH4}`,
      },
      reset_password: {
        post: (option?: { config?: T | undefined } | undefined) =>
          fetch<MethodsResetPassword['post']['resBody'], BasicHeaders, MethodsResetPassword['post']['status']>(
            prefix,
            PATH5,
            POST,
            option,
          ).json(),
        $post: (option?: { config?: T | undefined } | undefined) =>
          fetch<MethodsResetPassword['post']['resBody'], BasicHeaders, MethodsResetPassword['post']['status']>(
            prefix,
            PATH5,
            POST,
            option,
          )
            .json()
            .then(r => r.body),
        $path: () => `${prefix}${PATH5}`,
      },
      reset_password_by_credentials: {
        post: (option: { body: MethodsResetPasswordByCredentials['post']['reqBody'], config?: T | undefined }) =>
          fetch<
            MethodsResetPasswordByCredentials['post']['resBody'],
            BasicHeaders,
            MethodsResetPasswordByCredentials['post']['status']
          >(prefix, PATH6, POST, option).json(),
        $post: (option: { body: MethodsResetPasswordByCredentials['post']['reqBody'], config?: T | undefined }) =>
          fetch<
            MethodsResetPasswordByCredentials['post']['resBody'],
            BasicHeaders,
            MethodsResetPasswordByCredentials['post']['status']
          >(prefix, PATH6, POST, option)
            .json()
            .then(r => r.body),
        $path: () => `${prefix}${PATH6}`,
      },
      test_login: {
        post: (option: { body: MethodsTestLogin['post']['reqBody'], config?: T | undefined }) =>
          fetch<MethodsTestLogin['post']['resBody'], BasicHeaders, MethodsTestLogin['post']['status']>(
            prefix,
            PATH7,
            POST,
            option,
          ).json(),
        $post: (option: { body: MethodsTestLogin['post']['reqBody'], config?: T | undefined }) =>
          fetch<MethodsTestLogin['post']['resBody'], BasicHeaders, MethodsTestLogin['post']['status']>(
            prefix,
            PATH7,
            POST,
            option,
          )
            .json()
            .then(r => r.body),
        $path: () => `${prefix}${PATH7}`,
      },
      test_users: {
        get: (option?: { config?: T | undefined } | undefined) =>
          fetch<MethodsTestUsers['get']['resBody'], BasicHeaders, MethodsTestUsers['get']['status']>(prefix, PATH8, GET, option).json(),
        $get: (option?: { config?: T | undefined } | undefined) =>
          fetch<MethodsTestUsers['get']['resBody'], BasicHeaders, MethodsTestUsers['get']['status']>(prefix, PATH8, GET, option)
            .json()
            .then(r => r.body),
        $path: () => `${prefix}${PATH8}`,
      },
    },
  }
}

export type ApiInstance = ReturnType<typeof api>
export default api
