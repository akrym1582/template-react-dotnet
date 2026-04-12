import type { DefineMethods } from 'aspida'
import type { ApiResponse, TestLoginRequestDto, UserDto } from '../_types'

export type Methods = DefineMethods<{
  post: {
    reqBody: TestLoginRequestDto
    resBody: ApiResponse<UserDto>
    status: 200
  }
}>
