import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { adminLogin } from '../services/adminApi'

export const useAdminAuthStore = defineStore('adminAuth', () => {
  const token = ref<string | null>(localStorage.getItem('admin_token'))

  const isAuthenticated = computed(() => !!token.value)

  async function login(email: string, password: string) {
    const t = await adminLogin(email, password)
    token.value = t
    localStorage.setItem('admin_token', t)
  }

  function logout() {
    token.value = null
    localStorage.removeItem('admin_token')
  }

  return { token, isAuthenticated, login, logout }
})
