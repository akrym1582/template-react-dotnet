/** API レスポンスの共通型。成功フラグ・データ・メッセージを持つ。 */
export type ApiResponse<T> = {
  success: boolean
  data?: T | null
  message?: string | null
}

/**
 * 不明な型のレスポンスを `ApiResponse<T>` にキャストするユーティリティ関数。
 * aspida の NoThrow クライアントが返すレスポンスに対して使用する。
 * @param response - キャスト前のレスポンスオブジェクト
 * @returns `ApiResponse<T>` 型にキャストされたオブジェクト
 */
export const asApiResponse = <T>(response: unknown): ApiResponse<T> => response as ApiResponse<T>
