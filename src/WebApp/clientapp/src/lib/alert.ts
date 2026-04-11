import Swal from 'sweetalert2'

interface WithLoadingOptions {
  errorMessage?: string
  loadingMessage?: string
  loadingTitle?: string
}

export const alert = {
  success(message: string, title = '成功') {
    return Swal.fire({ icon: 'success', title, text: message })
  },

  error(message: string, title = 'エラー') {
    return Swal.fire({ icon: 'error', title, text: message })
  },

  warning(message: string, title = '警告') {
    return Swal.fire({ icon: 'warning', title, text: message })
  },

  info(message: string, title = '情報') {
    return Swal.fire({ icon: 'info', title, text: message })
  },

  async confirm(message: string, title = '確認') {
    const result = await Swal.fire({
      icon: 'question',
      title,
      text: message,
      showCancelButton: true,
      confirmButtonText: 'はい',
      cancelButtonText: 'キャンセル',
    })
    return result.isConfirmed
  },

  async withLoading<T>(action: () => Promise<T>, options: WithLoadingOptions = {}) {
    const {
      errorMessage,
      loadingMessage = 'しばらくお待ちください。',
      loadingTitle = '処理中',
    } = options

    void Swal.fire({
      title: loadingTitle,
      text: loadingMessage,
      allowOutsideClick: false,
      allowEscapeKey: false,
      showConfirmButton: false,
      didOpen: () => {
        Swal.showLoading()
      },
    })

    try {
      return await action()
    } catch (error) {
      const message =
        errorMessage ??
        (error instanceof Error && error.message
          ? error.message
          : '通信エラーが発生しました。')
      await alert.error(message)
      return undefined
    } finally {
      Swal.close()
    }
  },
}
