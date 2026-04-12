/* eslint-disable */
import type { DefineMethods } from 'aspida';
import type * as Types from '../../@types';

export type Methods = DefineMethods<{
  get: {
    status: 200;
    /** OK */
    resBody: Types.ApiResponseDtoOfUserListResponseDto;
  };

  post: {
    status: 200;
    /** OK */
    resBody: Types.ApiResponseDtoOfCreatedUserResultDto;
    reqBody: Types.CreateUserRequestDto;
  };
}>;
