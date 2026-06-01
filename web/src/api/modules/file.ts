import request from '@/api'
import { ElMessage } from 'element-plus'

const MAX_UPLOAD_BYTES = 5 * 1024 * 1024
const MAX_IMAGE_WIDTH = 1200
const ALLOWED_EXTENSIONS = ['jpg', 'jpeg', 'png', 'webp', 'gif']

const getExtension = (name: string) => name.split('.').pop()?.toLowerCase() || ''

const rejectUpload = (message: string) => {
  ElMessage.error(message)
  return Promise.reject(new Error(message))
}

const loadImage = (file: File) => new Promise<HTMLImageElement>((resolve, reject) => {
  const image = new Image()
  const url = URL.createObjectURL(file)
  image.onload = () => {
    URL.revokeObjectURL(url)
    resolve(image)
  }
  image.onerror = () => {
    URL.revokeObjectURL(url)
    reject(new Error('图片读取失败，请更换图片后重试'))
  }
  image.src = url
})

const canvasToBlob = (canvas: HTMLCanvasElement, type: string, quality: number) =>
  new Promise<Blob>((resolve, reject) => {
    canvas.toBlob((blob) => {
      if (blob) {
        resolve(blob)
      } else {
        reject(new Error('图片压缩失败，请更换图片后重试'))
      }
    }, type, quality)
  })

const compressImage = async (file: File, extension: string) => {
  if (extension === 'gif') return file

  const image = await loadImage(file)
  const scale = Math.min(1, MAX_IMAGE_WIDTH / image.naturalWidth)
  if (file.size <= MAX_UPLOAD_BYTES && scale === 1) return file

  const canvas = document.createElement('canvas')
  canvas.width = Math.max(1, Math.round(image.naturalWidth * scale))
  canvas.height = Math.max(1, Math.round(image.naturalHeight * scale))

  const context = canvas.getContext('2d')
  if (!context) throw new Error('图片压缩失败，请更换图片后重试')

  context.fillStyle = '#fff'
  context.fillRect(0, 0, canvas.width, canvas.height)
  context.drawImage(image, 0, 0, canvas.width, canvas.height)

  const qualities = [0.86, 0.78, 0.68, 0.58, 0.48]
  for (const quality of qualities) {
    const blob = await canvasToBlob(canvas, 'image/jpeg', quality)
    if (blob.size <= MAX_UPLOAD_BYTES || quality === qualities[qualities.length - 1]) {
      const name = file.name.replace(/\.[^.]+$/, '') || 'upload'
      return new File([blob], `${name}.jpg`, { type: 'image/jpeg', lastModified: Date.now() })
    }
  }

  return file
}

const prepareUploadFile = async (file: File) => {
  const extension = getExtension(file.name)
  if (!ALLOWED_EXTENSIONS.includes(extension)) {
    return rejectUpload('仅支持 JPG、PNG、WebP、GIF 图片')
  }

  let uploadFile: File
  try {
    uploadFile = await compressImage(file, extension)
  } catch (error) {
    if (file.size <= MAX_UPLOAD_BYTES) return file
    const message = error instanceof Error ? error.message : '图片压缩失败，请更换图片后重试'
    return rejectUpload(message)
  }

  if (uploadFile.size > MAX_UPLOAD_BYTES) {
    return rejectUpload('图片不能超过 5MB，请更换图片或先压缩后再上传')
  }

  return uploadFile
}

export const fileApi = {
  upload: async (file: File) => {
    const uploadFile = await prepareUploadFile(file)
    const data = new FormData()
    data.append('file', uploadFile)
    return request.post<{ url: string; fileName: string; size: number }>('/files/upload', data)
  },
}
