import type { DefineMethods } from 'aspida'
import type { ApiResponse, ChangePasswordRequestDto, UserDto } from '../_types'

export type Methods = DefineMethods<{
  post: {
    reqBody: ChangePasswordRequestDto
    resBody: ApiResponse<UserDto>
    status: 200
  }
}>
