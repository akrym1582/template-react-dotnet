import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

/**
 * clsx と tailwind-merge を組み合わせてクラス名を結合するユーティリティ関数。
 * 条件付きクラスや Tailwind クラスの競合を解決する。
 * @param inputs - 結合するクラス名または条件付きクラスオブジェクト
 * @returns 結合・最適化されたクラス名文字列
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
