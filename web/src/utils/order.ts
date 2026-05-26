import type { OrderDto, OrderItemDto } from '@/types/models/order'

const normalizeOrderItem = (raw: any): OrderItemDto => ({
  id: raw.id,
  productId: raw.productId,
  productName: raw.productName,
  productImage: raw.productImage,
  quantity: raw.quantity,
  unitPrice: Number(raw.unitPrice || 0),
  totalPrice: Number(raw.totalPrice || 0),
  specSnapshot: raw.specSnapshot,
  remark: raw.remark,
  isCombo: Boolean(raw.isCombo),
})

export const normalizeCustomerOrder = (raw: any, fallbackStoreName = ''): OrderDto => {
  const rawItems = raw.items || raw.orderItems || []

  return {
    id: raw.id,
    orderNo: raw.orderNo,
    storeId: raw.storeId,
    storeName: raw.storeName || raw.store?.name || fallbackStoreName,
    pickupCode: raw.pickupCode,
    diningMode: raw.diningMode,
    tableNo: raw.tableNo,
    totalAmount: Number(raw.totalAmount || 0),
    discountAmount: Number(raw.discountAmount || 0),
    actualAmount: Number(raw.actualAmount || 0),
    packingFee: Number(raw.packingFee || 0),
    remark: raw.remark,
    status: raw.status,
    items: rawItems.map(normalizeOrderItem),
    createdAt: raw.createdAt,
  }
}
