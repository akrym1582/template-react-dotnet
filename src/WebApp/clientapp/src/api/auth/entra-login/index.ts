import type { DefineMethods } from 'aspida'
import type { ApiResponse, EntraLoginRequestDto, UserDto } from '../_types'

export type Methods = DefineMethods<{
  post: {
    reqBody: EntraLoginRequestDto
    resBody: ApiResponse<UserDto>
    status: 200
  }
}>
