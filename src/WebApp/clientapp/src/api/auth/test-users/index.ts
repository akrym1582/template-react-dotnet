import type { DefineMethods } from 'aspida'
import type { ApiResponse, TestLoginUserDto } from '../_types'

export type Methods = DefineMethods<{
  get: {
    resBody: ApiResponse<TestLoginUserDto[]>
    status: 200
  }
}>
