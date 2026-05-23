import request from '@/api'

export const dashboardApi = {
  getOverview: (storeId: string) =>
    request.get<any>(`/dashboard/${storeId}/overview`),
}