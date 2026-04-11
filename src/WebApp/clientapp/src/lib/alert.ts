import Swal from 'sweetalert2'

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
}
