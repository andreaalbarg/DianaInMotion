<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAdminAuthStore } from '../../stores/adminAuth'

const auth = useAdminAuthStore()
const router = useRouter()
const password = ref('')
const error = ref('')
const loading = ref(false)

async function submit() {
  error.value = ''
  loading.value = true
  try {
    await auth.login(password.value)
    router.push('/admin/events')
  } catch {
    error.value = 'Invalid password.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-[#231f20] flex items-center justify-center">
    <div class="w-full max-w-sm">
      <div class="mb-8 text-center">
        <h1 class="text-2xl font-black text-white tracking-tight">DIANA / IN MOTION</h1>
        <p class="text-sm text-white/40 mt-1">Admin Panel</p>
      </div>
      <form @submit.prevent="submit" class="bg-white/5 border border-white/10 rounded-xl p-8 space-y-4">
        <div>
          <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-2">Password</label>
          <input
            v-model="password"
            type="password"
            placeholder="Enter admin password"
            class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-3 text-white placeholder-white/30 focus:outline-none focus:border-[#c2f33d] transition-colors"
          />
        </div>
        <p v-if="error" class="text-red-400 text-sm">{{ error }}</p>
        <button
          type="submit"
          :disabled="loading"
          class="w-full bg-[#c2f33d] text-[#231f20] font-black py-3 rounded-lg hover:bg-[#d4ff4a] transition-colors disabled:opacity-50"
        >
          {{ loading ? 'Signing in...' : 'Sign In' }}
        </button>
      </form>
    </div>
  </div>
</template>
