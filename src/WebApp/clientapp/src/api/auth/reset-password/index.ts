import type { DefineMethods } from 'aspida'
import type { ApiResponse, PasswordResetResultDto } from '../_types'

export type Methods = DefineMethods<{
  post: {
    resBody: ApiResponse<PasswordResetResultDto>
    status: 200
  }
}>
