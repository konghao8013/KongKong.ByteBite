import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/pages/Home.vue'),
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/pages/merchant/Login.vue'),
  },
  {
    path: '/merchant/login',
    redirect: '/login',
  },
  {
    path: '/admin/login',
    redirect: '/login',
  },
  {
    path: '/merchant',
    component: () => import('@/layouts/MerchantLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/merchant/orders' },
      { path: 'orders', name: 'MerchantOrders', component: () => import('@/pages/merchant/Orders.vue') },
      { path: 'menu', name: 'MerchantMenu', component: () => import('@/pages/merchant/Menu.vue') },
      { path: 'store', name: 'MerchantStore', component: () => import('@/pages/merchant/StoreInfo.vue') },
      { path: 'discounts', name: 'MerchantDiscounts', component: () => import('@/pages/merchant/Discounts.vue') },
      { path: 'dashboard', name: 'MerchantDashboard', component: () => import('@/pages/merchant/Dashboard.vue') },
    ],
  },
  {
    path: '/admin/login',
    name: 'AdminLogin',
    component: () => import('@/pages/admin/Login.vue'),
  },
  {
    path: '/admin',
    component: () => import('@/layouts/AdminLayout.vue'),
    meta: { requiresAdminAuth: true },
    children: [
      { path: '', redirect: '/admin/merchants' },
      { path: 'merchants', name: 'AdminMerchants', component: () => import('@/pages/admin/Merchants.vue') },
      { path: 'configs', name: 'AdminConfigs', component: () => import('@/pages/admin/Configs.vue') },
      { path: 'stats', name: 'AdminStats', component: () => import('@/pages/admin/Stats.vue') },
    ],
  },
  {
    path: '/store/:storeId',
    component: () => import('@/layouts/CustomerLayout.vue'),
    children: [
      { path: '', name: 'StoreMenu', component: () => import('@/pages/customer/StoreMenu.vue') },
      { path: 'cart', name: 'Cart', component: () => import('@/pages/customer/Cart.vue') },
      { path: 'product/:productId', name: 'ProductDetail', component: () => import('@/pages/customer/ProductDetail.vue') },
      { path: 'order/:orderId', name: 'OrderDetail', component: () => import('@/pages/customer/OrderDetail.vue') },
      { path: 'orders', name: 'MyOrders', component: () => import('@/pages/customer/MyOrders.vue') },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to, _from, next) => {
  if (to.meta.requiresAuth) {
    const token = localStorage.getItem('merchant_token')
    if (!token) {
      next({ name: 'Login' })
      return
    }
  }
  if (to.meta.requiresAdminAuth) {
    const token = localStorage.getItem('admin_token')
    if (!token) {
      next({ name: 'Login' })
      return
    }
  }
  next()
})

export default router