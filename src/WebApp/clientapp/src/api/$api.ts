import type { AspidaClient, BasicHeaders } from 'aspida';
import type { Methods as Methods_ac6duo } from './api/Auth/change-password';
import type { Methods as Methods_a4ur46 } from './api/Auth/entra-login';
import type { Methods as Methods_270u1f } from './api/Auth/login';
import type { Methods as Methods_1y0sj28 } from './api/Auth/logout';
import type { Methods as Methods_1pgikh8 } from './api/Auth/me';
import type { Methods as Methods_3j7cwd } from './api/Auth/reset-password';
import type { Methods as Methods_aahikm } from './api/Auth/reset-password-by-credentials';
import type { Methods as Methods_qu6uem } from './api/Auth/test-login';
import type { Methods as Methods_1le4j6n } from './api/Auth/test-users';
import type { Methods as Methods_xzne74 } from './api/User';
import type { Methods as Methods_10ziil1 } from './api/User/_userId@string';

const api = <T>({ baseURL, fetch }: AspidaClient<T>) => {
  const prefix = (baseURL === undefined ? '' : baseURL).replace(/\/$/, '');
  const PATH0 = '/api/Auth/change-password';
  const PATH1 = '/api/Auth/entra-login';
  const PATH2 = '/api/Auth/login';
  const PATH3 = '/api/Auth/logout';
  const PATH4 = '/api/Auth/me';
  const PATH5 = '/api/Auth/reset-password';
  const PATH6 = '/api/Auth/reset-password-by-credentials';
  const PATH7 = '/api/Auth/test-login';
  const PATH8 = '/api/Auth/test-users';
  const PATH9 = '/api/User';
  const GET = 'GET';
  const POST = 'POST';
  const PUT = 'PUT';
  const DELETE = 'DELETE';

  return {
    api: {
      Auth: {
        change_password: {
          /**
           * @returns OK
           */
          post: (option: { body: Methods_ac6duo['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_ac6duo['post']['resBody'], BasicHeaders, Methods_ac6duo['post']['status']>(prefix, PATH0, POST, option).json(),
          /**
           * @returns OK
           */
          $post: (option: { body: Methods_ac6duo['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_ac6duo['post']['resBody'], BasicHeaders, Methods_ac6duo['post']['status']>(prefix, PATH0, POST, option).json().then(r => r.body),
          $path: () => `${prefix}${PATH0}`,
        },
        entra_login: {
          /**
           * @returns OK
           */
          post: (option: { body: Methods_a4ur46['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_a4ur46['post']['resBody'], BasicHeaders, Methods_a4ur46['post']['status']>(prefix, PATH1, POST, option).json(),
          /**
           * @returns OK
           */
          $post: (option: { body: Methods_a4ur46['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_a4ur46['post']['resBody'], BasicHeaders, Methods_a4ur46['post']['status']>(prefix, PATH1, POST, option).json().then(r => r.body),
          $path: () => `${prefix}${PATH1}`,
        },
        login: {
          /**
           * @returns OK
           */
          post: (option: { body: Methods_270u1f['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_270u1f['post']['resBody'], BasicHeaders, Methods_270u1f['post']['status']>(prefix, PATH2, POST, option).json(),
          /**
           * @returns OK
           */
          $post: (option: { body: Methods_270u1f['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_270u1f['post']['resBody'], BasicHeaders, Methods_270u1f['post']['status']>(prefix, PATH2, POST, option).json().then(r => r.body),
          $path: () => `${prefix}${PATH2}`,
        },
        logout: {
          /**
           * @returns OK
           */
          post: (option?: { config?: T | undefined } | undefined) =>
            fetch<Methods_1y0sj28['post']['resBody'], BasicHeaders, Methods_1y0sj28['post']['status']>(prefix, PATH3, POST, option).json(),
          /**
           * @returns OK
           */
          $post: (option?: { config?: T | undefined } | undefined) =>
            fetch<Methods_1y0sj28['post']['resBody'], BasicHeaders, Methods_1y0sj28['post']['status']>(prefix, PATH3, POST, option).json().then(r => r.body),
          $path: () => `${prefix}${PATH3}`,
        },
        me: {
          /**
           * @returns OK
           */
          get: (option?: { config?: T | undefined } | undefined) =>
            fetch<Methods_1pgikh8['get']['resBody'], BasicHeaders, Methods_1pgikh8['get']['status']>(prefix, PATH4, GET, option).json(),
          /**
           * @returns OK
           */
          $get: (option?: { config?: T | undefined } | undefined) =>
            fetch<Methods_1pgikh8['get']['resBody'], BasicHeaders, Methods_1pgikh8['get']['status']>(prefix, PATH4, GET, option).json().then(r => r.body),
          $path: () => `${prefix}${PATH4}`,
        },
        reset_password: {
          /**
           * @returns OK
           */
          post: (option?: { config?: T | undefined } | undefined) =>
            fetch<Methods_3j7cwd['post']['resBody'], BasicHeaders, Methods_3j7cwd['post']['status']>(prefix, PATH5, POST, option).json(),
          /**
           * @returns OK
           */
          $post: (option?: { config?: T | undefined } | undefined) =>
            fetch<Methods_3j7cwd['post']['resBody'], BasicHeaders, Methods_3j7cwd['post']['status']>(prefix, PATH5, POST, option).json().then(r => r.body),
          $path: () => `${prefix}${PATH5}`,
        },
        reset_password_by_credentials: {
          /**
           * @returns OK
           */
          post: (option: { body: Methods_aahikm['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_aahikm['post']['resBody'], BasicHeaders, Methods_aahikm['post']['status']>(prefix, PATH6, POST, option).json(),
          /**
           * @returns OK
           */
          $post: (option: { body: Methods_aahikm['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_aahikm['post']['resBody'], BasicHeaders, Methods_aahikm['post']['status']>(prefix, PATH6, POST, option).json().then(r => r.body),
          $path: () => `${prefix}${PATH6}`,
        },
        test_login: {
          /**
           * @returns OK
           */
          post: (option: { body: Methods_qu6uem['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_qu6uem['post']['resBody'], BasicHeaders, Methods_qu6uem['post']['status']>(prefix, PATH7, POST, option).json(),
          /**
           * @returns OK
           */
          $post: (option: { body: Methods_qu6uem['post']['reqBody'], config?: T | undefined }) =>
            fetch<Methods_qu6uem['post']['resBody'], BasicHeaders, Methods_qu6uem['post']['status']>(prefix, PATH7, POST, option).json().then(r => r.body),
          $path: () => `${prefix}${PATH7}`,
        },
        test_users: {
          /**
           * @returns OK
           */
          get: (option?: { config?: T | undefined } | undefined) =>
            fetch<Methods_1le4j6n['get']['resBody'], BasicHeaders, Methods_1le4j6n['get']['status']>(prefix, PATH8, GET, option).json(),
          /**
           * @returns OK
           */
          $get: (option?: { config?: T | undefined } | undefined) =>
            fetch<Methods_1le4j6n['get']['resBody'], BasicHeaders, Methods_1le4j6n['get']['status']>(prefix, PATH8, GET, option).json().then(r => r.body),
          $path: () => `${prefix}${PATH8}`,
        },
      },
      User: {
        _userId: (val2: string) => {
          const prefix2 = `${PATH9}/${val2}`;

          return {
            /**
             * @returns OK
             */
            get: (option?: { config?: T | undefined } | undefined) =>
              fetch<Methods_10ziil1['get']['resBody'], BasicHeaders, Methods_10ziil1['get']['status']>(prefix, prefix2, GET, option).json(),
            /**
             * @returns OK
             */
            $get: (option?: { config?: T | undefined } | undefined) =>
              fetch<Methods_10ziil1['get']['resBody'], BasicHeaders, Methods_10ziil1['get']['status']>(prefix, prefix2, GET, option).json().then(r => r.body),
            /**
             * @returns OK
             */
            put: (option: { body: Methods_10ziil1['put']['reqBody'], config?: T | undefined }) =>
              fetch<Methods_10ziil1['put']['resBody'], BasicHeaders, Methods_10ziil1['put']['status']>(prefix, prefix2, PUT, option).json(),
            /**
             * @returns OK
             */
            $put: (option: { body: Methods_10ziil1['put']['reqBody'], config?: T | undefined }) =>
              fetch<Methods_10ziil1['put']['resBody'], BasicHeaders, Methods_10ziil1['put']['status']>(prefix, prefix2, PUT, option).json().then(r => r.body),
            /**
             * @returns OK
             */
            delete: (option?: { config?: T | undefined } | undefined) =>
              fetch<Methods_10ziil1['delete']['resBody'], BasicHeaders, Methods_10ziil1['delete']['status']>(prefix, prefix2, DELETE, option).json(),
            /**
             * @returns OK
             */
            $delete: (option?: { config?: T | undefined } | undefined) =>
              fetch<Methods_10ziil1['delete']['resBody'], BasicHeaders, Methods_10ziil1['delete']['status']>(prefix, prefix2, DELETE, option).json().then(r => r.body),
            $path: () => `${prefix}${prefix2}`,
          };
        },
        /**
         * @returns OK
         */
        get: (option?: { config?: T | undefined } | undefined) =>
          fetch<Methods_xzne74['get']['resBody'], BasicHeaders, Methods_xzne74['get']['status']>(prefix, PATH9, GET, option).json(),
        /**
         * @returns OK
         */
        $get: (option?: { config?: T | undefined } | undefined) =>
          fetch<Methods_xzne74['get']['resBody'], BasicHeaders, Methods_xzne74['get']['status']>(prefix, PATH9, GET, option).json().then(r => r.body),
        /**
         * @returns OK
         */
        post: (option: { body: Methods_xzne74['post']['reqBody'], config?: T | undefined }) =>
          fetch<Methods_xzne74['post']['resBody'], BasicHeaders, Methods_xzne74['post']['status']>(prefix, PATH9, POST, option).json(),
        /**
         * @returns OK
         */
        $post: (option: { body: Methods_xzne74['post']['reqBody'], config?: T | undefined }) =>
          fetch<Methods_xzne74['post']['resBody'], BasicHeaders, Methods_xzne74['post']['status']>(prefix, PATH9, POST, option).json().then(r => r.body),
        $path: () => `${prefix}${PATH9}`,
      },
    },
  };
};

export type ApiInstance = ReturnType<typeof api>;
export default api;
