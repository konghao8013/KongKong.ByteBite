import request from '@/api'
import type { TemplateDto, IndustryCategoryDto, ApplyTemplateRequest } from '@/types/models/template'

export const templateApi = {
  getList: (industryCategoryId?: string) =>
    request.get<TemplateDto[]>('/templates', { params: { industryCategoryId } }),

  getById: (id: string) =>
    request.get<TemplateDto>(`/templates/${id}`),

  createFromScratch: (data: { name: string; industryCategoryId?: string; coverImageUrl?: string; description?: string; status?: string }) =>
    request.post<TemplateDto>('/templates/from-scratch', data),

  createFromStore: (data: {
    storeId: string
    name: string
    industryCategoryId: string
    includeAll?: boolean
    selectedCategoryIds?: string[]
    selectedProductIds?: string[]
  }) =>
    request.post<TemplateDto>('/templates/from-store', data),

  createCombined: (data: {
    name: string
    industryCategoryId: string
    storeSelections: { storeId: string; includeAll: boolean; selectedCategoryIds?: string[]; selectedProductIds?: string[] }[]
  }) =>
    request.post<TemplateDto>('/templates/combined', data),

  update: (id: string, data: { name?: string; description?: string; status?: string }) =>
    request.put<TemplateDto>(`/templates/${id}`, data),

  addCategory: (templateId: string, data: { name: string; categoryType?: string; icon?: string; sortOrder?: number }) =>
    request.post<void>(`/templates/${templateId}/categories`, data),

  addProduct: (templateId: string, data: {
    categoryId: string
    name: string
    referencePrice: number
    description?: string
    imageUrl?: string
    minOrderQty?: number
    specGroups?: { name: string; isRequired: boolean; options: { name: string; extraPrice: number; isDefault: boolean }[] }[]
  }) =>
    request.post<void>(`/templates/${templateId}/products`, data),

  removeCategory: (templateId: string, categoryId: string) =>
    request.delete<void>(`/templates/${templateId}/categories/${categoryId}`),

  removeProduct: (templateId: string, productId: string) =>
    request.delete<void>(`/templates/${templateId}/products/${productId}`),

  applyTemplate: (data: ApplyTemplateRequest) =>
    request.post<void>('/templates/apply', data),
}

export const industryCategoryApi = {
  getTree: () =>
    request.get<IndustryCategoryDto[]>('/industry-categories/tree'),

  create: (data: { name: string; parentId?: string; icon?: string; sortOrder?: number }) =>
    request.post<IndustryCategoryDto>('/industry-categories', data),

  update: (id: string, data: { name?: string; icon?: string; sortOrder?: number }) =>
    request.put<IndustryCategoryDto>(`/industry-categories/${id}`, data),

  delete: (id: string) =>
    request.delete<void>(`/industry-categories/${id}`),
}
