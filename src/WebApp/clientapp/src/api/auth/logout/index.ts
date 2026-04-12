import type { DefineMethods } from 'aspida'
import type { ApiResponse } from '../_types'

export type Methods = DefineMethods<{
  post: {
    resBody: ApiResponse<void>
    status: 200
  }
}>
