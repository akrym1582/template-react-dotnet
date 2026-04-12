import type { DefineMethods } from 'aspida'
import type { ApiResponse, PasswordResetResultDto, ResetPasswordByCredentialsRequestDto } from '../_types'

export type Methods = DefineMethods<{
  post: {
    reqBody: ResetPasswordByCredentialsRequestDto
    resBody: ApiResponse<PasswordResetResultDto>
    status: 200
  }
}>
