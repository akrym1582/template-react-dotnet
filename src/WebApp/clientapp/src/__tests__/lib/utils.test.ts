import { cn } from '@/lib/utils'

describe('cn', () => {
  it('単一クラス名を返す', () => {
    expect(cn('foo')).toBe('foo')
  })

  it('複数クラス名を結合する', () => {
    expect(cn('foo', 'bar')).toBe('foo bar')
  })

  it('falsy な値を無視する', () => {
    expect(cn('foo', undefined, null, false, 'bar')).toBe('foo bar')
  })

  it('条件付きクラスを処理する', () => {
    expect(cn('base', { active: true, disabled: false })).toBe('base active')
  })

  it('Tailwind の競合クラスをマージする', () => {
    expect(cn('p-4', 'p-2')).toBe('p-2')
  })

  it('引数なしで空文字を返す', () => {
    expect(cn()).toBe('')
  })
})
