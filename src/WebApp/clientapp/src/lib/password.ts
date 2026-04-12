import { alert } from '@/lib/alert'

/**
 * 初期パスワードをクリップボードへコピーし、結果に応じたアラートを表示する。
 * クリップボードへのコピーに成功した場合は `successMessage`、
 * 未対応環境などで失敗した場合は `fallbackMessage` をアラートで表示する。
 * @param initialPassword - コピーする初期パスワード文字列。`undefined` の場合は `fallbackMessage` を表示する
 * @param successMessage - クリップボードコピー成功時に表示するメッセージ
 * @param fallbackMessage - クリップボード未対応などコピー失敗時に表示するメッセージ
 */
export async function notifyInitialPassword(
  initialPassword: string | undefined,
  successMessage: string,
  fallbackMessage: string,
) {
  if (initialPassword) {
    try {
      await navigator.clipboard.writeText(initialPassword)
      await alert.success(successMessage)
      return
    } catch {
      // クリップボード未対応環境では設定値の確認に誘導する
    }
  }

  await alert.success(fallbackMessage)
}
