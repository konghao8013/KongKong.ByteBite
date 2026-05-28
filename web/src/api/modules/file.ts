import request from '@/api'

export const fileApi = {
  upload: (file: File) => {
    const data = new FormData()
    data.append('file', file)
    return request.post<{ url: string; fileName: string; size: number }>('/files/upload', data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
}
