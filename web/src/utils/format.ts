/** 格式化价格为人民币显示 */
export function formatPrice(price: number): string {
  return `¥${price.toFixed(2)}`
}

/** 格式化日期为可读字符串 */
export function formatDate(dateStr: string, format: string = 'YYYY-MM-DD HH:mm'): string {
  const date = new Date(dateStr)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  const seconds = String(date.getSeconds()).padStart(2, '0')

  return format
    .replace('YYYY', String(year))
    .replace('MM', month)
    .replace('DD', day)
    .replace('HH', hours)
    .replace('mm', minutes)
    .replace('ss', seconds)
}

/** 生成设备唯一标识 */
export function generateDeviceId(): string {
  const timestamp = Date.now().toString(36)
  const random = Math.random().toString(36).substring(2, 10)
  return `dev_${timestamp}_${random}`
}
