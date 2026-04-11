import { alert } from '@/lib/alert'

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
