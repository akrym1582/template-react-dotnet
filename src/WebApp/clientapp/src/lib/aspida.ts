import aspida from '@aspida/fetch'

const fetchConfig = {
  baseURL: '/api',
  throwHttpErrors: true,
}

export const aspidaClient = aspida(fetch, fetchConfig)
