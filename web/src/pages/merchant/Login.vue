<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import request from '@/api'
import { storeApi } from '@/api/modules/store'
import { ElMessage } from 'element-plus'

const router = useRouter()
const loading = ref(false)
const isRegister = ref(false)

const loginForm = ref({ account: '', password: '' })
const registerForm = ref({ phone: '', password: '', confirmPassword: '', storeName: '' })

const handleLogin = async () => {
  if (!loginForm.value.account || !loginForm.value.password) {
    ElMessage.warning('请输入账号和密码')
    return
  }
  loading.value = true
  try {
    const res = await request.post<any>('/auth/login', {
      account: loginForm.value.account,
      password: loginForm.value.password,
    })
    if (res) {
      const role = res.role
      const data = res.data

      if (role === 'admin') {
        localStorage.setItem('admin_token', data.token)
        localStorage.setItem('admin_id', data.id)
        localStorage.setItem('admin_info', JSON.stringify(data))
        ElMessage.success('管理员登录成功')
        router.push('/admin/merchants')
      } else if (role === 'merchant') {
        localStorage.setItem('merchant_token', data.token)
        localStorage.setItem('merchant_id', data.id)
        localStorage.setItem('merchant_info', JSON.stringify(data))
        if (res.storeId) {
          localStorage.setItem('merchant_store_id', res.storeId)
        } else {
          try {
            const stores = await storeApi.getByMerchantId(data.id)
            if (stores && stores.length > 0) {
              localStorage.setItem('merchant_store_id', stores[0].id)
            }
          } catch { /* ignore */ }
        }
        ElMessage.success('商家登录成功')
        router.push('/merchant/orders')
      } else if (role === 'customer') {
        localStorage.setItem('customer_token', data.token)
        localStorage.setItem('customer_id', data.id)
        localStorage.setItem('customer_info', JSON.stringify(data))
        ElMessage.success('登录成功')
        router.push('/')
      }
    }
  } catch {
    ElMessage.error('账号或密码错误')
  } finally {
    loading.value = false
  }
}

const handleRegister = async () => {
  const { phone, password, confirmPassword, storeName } = registerForm.value
  if (!phone || !password || !storeName) {
    ElMessage.warning('请填写完整信息')
    return
  }
  if (password !== confirmPassword) {
    ElMessage.warning('两次密码输入不一致')
    return
  }
  if (!/^1[3-9]\d{9}$/.test(phone)) {
    ElMessage.warning('请输入正确的手机号')
    return
  }
  loading.value = true
  try {
    const res = await request.post<any>('/merchants/register', { phone, password, storeName })
    if (res) {
      ElMessage.success('注册成功，请登录')
      loginForm.value.account = phone
      isRegister.value = false
    }
  } catch {
    ElMessage.error('注册失败，该手机号可能已注册')
  } finally {
    loading.value = false
  }
}

const switchMode = () => { isRegister.value = !isRegister.value }
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <div class="login-header">
        <span class="login-icon">🍔</span>
        <h2>{{ isRegister ? '注册商家账号' : '空空码上点单' }}</h2>
        <p v-if="!isRegister">输入账号自动识别身份</p>
        <p v-else>注册后需管理员审核</p>
      </div>

      <div v-if="!isRegister" class="login-form">
        <div class="form-group">
          <label>账号</label>
          <input
            v-model="loginForm.account"
            type="text"
            placeholder="手机号 / 管理员用户名"
            @keyup.enter="handleLogin"
          />
        </div>
        <div class="form-group">
          <label>密码</label>
          <input
            v-model="loginForm.password"
            type="password"
            placeholder="请输入密码"
            @keyup.enter="handleLogin"
          />
        </div>
        <div class="role-hint">
          <span class="hint-item">📱 手机号 → 商家/顾客</span>
          <span class="hint-item">👤 用户名 → 管理员</span>
        </div>
        <button class="login-btn" :disabled="loading" @click="handleLogin">
          {{ loading ? '登录中...' : '登 录' }}
        </button>
        <div class="switch-mode">
          还没有商家账号？<span @click="switchMode">立即注册</span>
        </div>
      </div>

      <div v-else class="login-form">
        <div class="form-group">
          <label>手机号</label>
          <input v-model="registerForm.phone" type="tel" placeholder="请输入手机号" maxlength="11" />
        </div>
        <div class="form-group">
          <label>密码</label>
          <input v-model="registerForm.password" type="password" placeholder="请设置密码（至少6位）" />
        </div>
        <div class="form-group">
          <label>确认密码</label>
          <input v-model="registerForm.confirmPassword" type="password" placeholder="请再次输入密码" />
        </div>
        <div class="form-group">
          <label>店铺名称</label>
          <input v-model="registerForm.storeName" type="text" placeholder="请输入店铺名称" />
        </div>
        <button class="login-btn" :disabled="loading" @click="handleRegister">
          {{ loading ? '注册中...' : '注 册' }}
        </button>
        <div class="switch-mode">
          已有账号？<span @click="switchMode">返回登录</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #FFD161 0%, #FF6633 100%);
  padding: 24px;
}

.login-card {
  width: 100%;
  max-width: 400px;
  background: #fff;
  border-radius: 16px;
  padding: 40px 32px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
}

.login-header {
  text-align: center;
  margin-bottom: 32px;

  .login-icon { font-size: 48px; display: block; margin-bottom: 12px; }

  h2 { font-size: 24px; font-weight: 700; color: #333; margin: 0 0 8px; }
  p { font-size: 14px; color: #999; margin: 0; }
}

.login-form {
  .form-group {
    margin-bottom: 20px;

    label { display: block; font-size: 14px; font-weight: 500; color: #333; margin-bottom: 8px; }

    input {
      width: 100%;
      height: 44px;
      padding: 0 16px;
      border: 1px solid #ddd;
      border-radius: 8px;
      font-size: 15px;
      outline: none;
      transition: border-color 0.2s;
      box-sizing: border-box;

      &:focus { border-color: #FF6633; box-shadow: 0 0 0 3px rgba(255, 102, 51, 0.15); }
      &::placeholder { color: #bbb; }
    }
  }
}

.role-hint {
  display: flex;
  gap: 16px;
  margin-bottom: 20px;
  padding: 10px 12px;
  background: #FFF8F0;
  border-radius: 8px;
  border: 1px solid #FFE0B2;

  .hint-item { font-size: 12px; color: #FF9800; }
}

.login-btn {
  width: 100%;
  height: 48px;
  background: linear-gradient(135deg, #FF6633, #FF4411);
  color: #fff;
  font-size: 16px;
  font-weight: 600;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: opacity 0.2s;

  &:hover { opacity: 0.9; }
  &:disabled { opacity: 0.6; cursor: not-allowed; }
}

.switch-mode {
  text-align: center;
  margin-top: 20px;
  font-size: 14px;
  color: #999;

  span { color: #FF6633; cursor: pointer; font-weight: 500; &:hover { text-decoration: underline; } }
}
</style>