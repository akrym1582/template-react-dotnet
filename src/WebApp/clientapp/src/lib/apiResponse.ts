export type ApiResponse<T> = {
  success: boolean
  data?: T | null
  message?: string | null
}

export const asApiResponse = <T>(response: unknown): ApiResponse<T> => response as ApiResponse<T>
