import type { AspidaClient, BasicHeaders } from 'aspida'
import type { Methods as MethodsEntraLogin } from './auth/entra-login'
import type { Methods as MethodsLogin } from './auth/login'
import type { Methods as MethodsLogout } from './auth/logout'
import type { Methods as MethodsMe } from './auth/me'

const api = <T>({ baseURL, fetch }: AspidaClient<T>) => {
  const prefix = (baseURL === undefined ? '' : baseURL).replace(/\/$/, '')
  const PATH0 = '/auth/entra-login'
  const PATH1 = '/auth/login'
  const PATH2 = '/auth/logout'
  const PATH3 = '/auth/me'
  const GET = 'GET'
  const POST = 'POST'

  return {
    auth: {
      entra_login: {
        post: (option?: { config?: T | undefined } | undefined) =>
          fetch<void, BasicHeaders, MethodsEntraLogin['post']['status']>(prefix, PATH0, POST, option).send(),
        $post: (option?: { config?: T | undefined } | undefined) =>
          fetch<void, BasicHeaders, MethodsEntraLogin['post']['status']>(prefix, PATH0, POST, option)
            .send()
            .then(r => r.body),
        $path: () => `${prefix}${PATH0}`,
      },
      login: {
        post: (option?: { config?: T | undefined } | undefined) =>
          fetch<void, BasicHeaders, MethodsLogin['post']['status']>(prefix, PATH1, POST, option).send(),
        $post: (option?: { config?: T | undefined } | undefined) =>
          fetch<void, BasicHeaders, MethodsLogin['post']['status']>(prefix, PATH1, POST, option)
            .send()
            .then(r => r.body),
        $path: () => `${prefix}${PATH1}`,
      },
      logout: {
        post: (option?: { config?: T | undefined } | undefined) =>
          fetch<void, BasicHeaders, MethodsLogout['post']['status']>(prefix, PATH2, POST, option).send(),
        $post: (option?: { config?: T | undefined } | undefined) =>
          fetch<void, BasicHeaders, MethodsLogout['post']['status']>(prefix, PATH2, POST, option)
            .send()
            .then(r => r.body),
        $path: () => `${prefix}${PATH2}`,
      },
      me: {
        get: (option?: { config?: T | undefined } | undefined) =>
          fetch<void, BasicHeaders, MethodsMe['get']['status']>(prefix, PATH3, GET, option).send(),
        $get: (option?: { config?: T | undefined } | undefined) =>
          fetch<void, BasicHeaders, MethodsMe['get']['status']>(prefix, PATH3, GET, option)
            .send()
            .then(r => r.body),
        $path: () => `${prefix}${PATH3}`,
      },
    },
  }
}

export type ApiInstance = ReturnType<typeof api>
export default api
