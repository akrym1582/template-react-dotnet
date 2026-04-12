import Swal from 'sweetalert2'

/** `alert.withLoading` に渡すオプション */
interface WithLoadingOptions {
  /** エラー発生時に表示するメッセージ。省略時はエラーオブジェクトのメッセージを使用する。 */
  errorMessage?: string
  /** ローディング中に表示するテキスト */
  loadingMessage?: string
  /** ローディング中に表示するタイトル */
  loadingTitle?: string
}

/**
 * SweetAlert2 をラップしたアラートユーティリティ。
 * 直接 SweetAlert2 を呼び出す代わりにこのオブジェクトを使用する。
 */
export const alert = {
  /**
   * 成功アラートを表示する。
   * @param message - 表示するメッセージ
   * @param title - タイトル（デフォルト: `'成功'`）
   */
  success(message: string, title = '成功') {
    return Swal.fire({ icon: 'success', title, text: message })
  },

  /**
   * エラーアラートを表示する。
   * @param message - 表示するメッセージ
   * @param title - タイトル（デフォルト: `'エラー'`）
   */
  error(message: string, title = 'エラー') {
    return Swal.fire({ icon: 'error', title, text: message })
  },

  /**
   * 警告アラートを表示する。
   * @param message - 表示するメッセージ
   * @param title - タイトル（デフォルト: `'警告'`）
   */
  warning(message: string, title = '警告') {
    return Swal.fire({ icon: 'warning', title, text: message })
  },

  /**
   * 情報アラートを表示する。
   * @param message - 表示するメッセージ
   * @param title - タイトル（デフォルト: `'情報'`）
   */
  info(message: string, title = '情報') {
    return Swal.fire({ icon: 'info', title, text: message })
  },

  /**
   * 確認ダイアログを表示し、ユーザーの選択結果を返す。
   * @param message - 確認内容のメッセージ
   * @param title - タイトル（デフォルト: `'確認'`）
   * @returns ユーザーが「はい」を選択した場合は `true`、キャンセルした場合は `false`
   */
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

  /**
   * 非同期処理の実行中にローディングダイアログを表示する。
   * 処理が完了するとダイアログを閉じる。エラーが発生した場合はエラーアラートを表示して `undefined` を返す。
   * @param action - ローディング中に実行する非同期処理
   * @param options - ローディングダイアログのオプション
   * @returns 処理結果。エラー発生時は `undefined`
   */
  async withLoading<T>(
    action: () => Promise<T>,
    options: WithLoadingOptions = {},
  ): Promise<T | undefined> {
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
