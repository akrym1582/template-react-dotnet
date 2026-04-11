export const roleOptions = [
  { value: 'general', label: '一般' },
  { value: 'manager', label: '役席' },
  { value: 'privileged', label: '特権' },
] as const

export function formatRole(role: string) {
  return roleOptions.find((option) => option.value === role)?.label ?? role
}

export function canManageUsers(roles: string[]) {
  return roles.includes('manager') || roles.includes('privileged')
}
