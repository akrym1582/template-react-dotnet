/* eslint-disable */
export type ApiResponseDto = {
  success: boolean;
}

export type ApiResponseDtoOfCreatedUserResultDto = {
  success: boolean;

  data?: null | CreatedUserResultDto | undefined;
}

export type ApiResponseDtoOfIReadOnlyListOfTestLoginUserDto = {
  success: boolean;
}

export type ApiResponseDtoOfPasswordResetResultDto = {
  success: boolean;

  data?: null | PasswordResetResultDto | undefined;
}

export type ApiResponseDtoOfUserDto = {
  success: boolean;

  data?: null | UserDto | undefined;
}

export type ApiResponseDtoOfUserListResponseDto = {
  success: boolean;

  data?: null | UserListResponseDto | undefined;
}

export type ChangePasswordRequestDto = {
  newPassword: string;
}

export type CreatedUserResultDto = {
  user: UserDto;
  initialPassword: string;
  mustChangePassword: boolean;
}

export type CreateUserRequestDto = {
  email: string;
  displayName: string;
  storeCode: string;
  storeName: string;
}

export type EntraLoginRequestDto = {
  idToken: string;
}

export type LoginRequestDto = {
  email: string;
  password: string;
}

export type PasswordResetResultDto = {
  initialPassword: string;
  mustChangePassword: boolean;
}

export type ResetPasswordByCredentialsRequestDto = {
  email: string;
  currentPassword: string;
}

export type TestLoginRequestDto = {
  userId: string;
}

export type TestLoginUserDto = {
  userId: string;
  roles: string[];
}

export type UpdateUserRequestDto = {
  email: string;
  displayName: string;
  storeCode: string;
  storeName: string;
}

export type UserDto = {
  userId: string;
  email: string;
  displayName: string;
  storeCode: string;
  storeName: string;
  roles: string[];
  isActive: boolean;
  mustChangePassword: boolean;
}

export type UserListResponseDto = {
  users: UserDto[];
  allowUserCreation: boolean;
}
