import { alert } from '@/lib/alert'

vi.mock('sweetalert2', () => ({
  default: {
    fire: vi.fn().mockResolvedValue({ isConfirmed: false }),
    showLoading: vi.fn(),
    close: vi.fn(),
  },
}))

import Swal from 'sweetalert2'

const mockFire = vi.mocked(Swal.fire)
const mockClose = vi.mocked(Swal.close)

beforeEach(() => {
  vi.clearAllMocks()
})

describe('alert.success', () => {
  it('成功アイコンで Swal.fire を呼び出す', async () => {
    mockFire.mockResolvedValueOnce({ isConfirmed: true } as never)
    await alert.success('完了しました。')
    expect(mockFire).toHaveBeenCalledWith(
      expect.objectContaining({ icon: 'success', text: '完了しました。' }),
    )
  })

  it('カスタムタイトルを使用できる', async () => {
    mockFire.mockResolvedValueOnce({ isConfirmed: true } as never)
    await alert.success('メッセージ', 'カスタムタイトル')
    expect(mockFire).toHaveBeenCalledWith(
      expect.objectContaining({ title: 'カスタムタイトル' }),
    )
  })
})

describe('alert.error', () => {
  it('エラーアイコンで Swal.fire を呼び出す', async () => {
    mockFire.mockResolvedValueOnce({ isConfirmed: false } as never)
    await alert.error('エラーが発生しました。')
    expect(mockFire).toHaveBeenCalledWith(
      expect.objectContaining({ icon: 'error', text: 'エラーが発生しました。' }),
    )
  })
})

describe('alert.warning', () => {
  it('警告アイコンで Swal.fire を呼び出す', async () => {
    mockFire.mockResolvedValueOnce({ isConfirmed: false } as never)
    await alert.warning('注意してください。')
    expect(mockFire).toHaveBeenCalledWith(
      expect.objectContaining({ icon: 'warning', text: '注意してください。' }),
    )
  })
})

describe('alert.info', () => {
  it('情報アイコンで Swal.fire を呼び出す', async () => {
    mockFire.mockResolvedValueOnce({ isConfirmed: false } as never)
    await alert.info('情報です。')
    expect(mockFire).toHaveBeenCalledWith(
      expect.objectContaining({ icon: 'info', text: '情報です。' }),
    )
  })
})

describe('alert.confirm', () => {
  it('確認ダイアログで true を返す (isConfirmed)', async () => {
    mockFire.mockResolvedValueOnce({ isConfirmed: true } as never)
    const result = await alert.confirm('削除しますか？')
    expect(result).toBe(true)
  })

  it('確認ダイアログでキャンセルされた場合 false を返す', async () => {
    mockFire.mockResolvedValueOnce({ isConfirmed: false } as never)
    const result = await alert.confirm('削除しますか？')
    expect(result).toBe(false)
  })
})

describe('alert.withLoading', () => {
  it('成功時にアクションの戻り値を返す', async () => {
    mockFire.mockResolvedValue({ isConfirmed: false } as never)
    const action = vi.fn().mockResolvedValue({ success: true })
    const result = await alert.withLoading(action)
    expect(result).toEqual({ success: true })
    expect(mockClose).toHaveBeenCalled()
  })

  it('エラー時に alert.error を表示して undefined を返す', async () => {
    mockFire.mockResolvedValue({ isConfirmed: false } as never)
    const action = vi.fn().mockRejectedValue(new Error('通信エラー'))
    const result = await alert.withLoading(action)
    expect(result).toBeUndefined()
    expect(mockFire).toHaveBeenCalledWith(expect.objectContaining({ icon: 'error' }))
  })

  it('カスタムエラーメッセージを使用できる', async () => {
    mockFire.mockResolvedValue({ isConfirmed: false } as never)
    const action = vi.fn().mockRejectedValue(new Error('original'))
    await alert.withLoading(action, { errorMessage: 'カスタムエラー' })
    expect(mockFire).toHaveBeenCalledWith(
      expect.objectContaining({ icon: 'error', text: 'カスタムエラー' }),
    )
  })

  it('ローディング中は Swal.fire でローディング画面を表示する', async () => {
    mockFire.mockResolvedValue({ isConfirmed: false } as never)
    const action = vi.fn().mockResolvedValue(null)
    await alert.withLoading(action)
    expect(mockFire).toHaveBeenCalledWith(
      expect.objectContaining({ title: '処理中', allowOutsideClick: false }),
    )
  })
})
