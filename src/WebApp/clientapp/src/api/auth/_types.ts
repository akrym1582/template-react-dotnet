export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
}

export interface UserDto {
  userId: string
  email: string
  displayName: string
  storeCode: string
  storeName: string
  roles: string[]
  isActive: boolean
  mustChangePassword: boolean
}

export interface TestLoginUserDto {
  userId: string
  roles: string[]
}

export interface PasswordResetResultDto {
  initialPassword: string
  mustChangePassword: boolean
}

export interface LoginRequestDto {
  email: string
  password: string
}

export interface EntraLoginRequestDto {
  idToken: string
}

export interface TestLoginRequestDto {
  userId: string
}

export interface ChangePasswordRequestDto {
  newPassword: string
}

export interface ResetPasswordByCredentialsRequestDto {
  email: string
  currentPassword: string
}
