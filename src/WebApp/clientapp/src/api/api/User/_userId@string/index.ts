/* eslint-disable */
import type { DefineMethods } from 'aspida';
import type * as Types from '../../../@types';

export type Methods = DefineMethods<{
  get: {
    status: 200;
    /** OK */
    resBody: Types.ApiResponseDtoOfUserDto;
  };

  put: {
    status: 200;
    /** OK */
    resBody: Types.ApiResponseDtoOfUserDto;
    reqBody: Types.UpdateUserRequestDto;
  };

  delete: {
    status: 200;
    /** OK */
    resBody: Types.ApiResponseDto;
  };
}>;
