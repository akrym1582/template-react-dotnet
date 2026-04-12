/** ロール選択肢の一覧。value はシステム内部値、label は表示名。 */
export const roleOptions = [
  { value: 'general', label: '一般' },
  { value: 'manager', label: '役席' },
  { value: 'privileged', label: '特権' },
] as const

/**
 * ロール値（英語）を日本語の表示名に変換する。
 * 対応する表示名が見つからない場合は元の値をそのまま返す。
 * @param role - 変換するロール値（例: `'general'`）
 * @returns 日本語の表示名（例: `'一般'`）
 */
export function formatRole(role: string) {
  return roleOptions.find((option) => option.value === role)?.label ?? role
}

/**
 * 指定したロール一覧がユーザー管理操作（役席以上）を行えるか判定する。
 * `manager` または `privileged` ロールを持つ場合に `true` を返す。
 * @param roles - 判定対象のロール一覧
 * @returns ユーザー管理可能な場合は `true`
 */
export function canManageUsers(roles: string[]) {
  return roles.includes('manager') || roles.includes('privileged')
}
