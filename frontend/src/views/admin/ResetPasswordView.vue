<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { adminResetPassword } from '../../services/adminApi'

const router = useRouter()
const route = useRoute()

const token = ref('')
const newPassword = ref('')
const confirm = ref('')
const loading = ref(false)
const done = ref(false)
const error = ref('')

onMounted(() => {
  token.value = route.query.token as string ?? ''
  if (!token.value) error.value = 'Enlace inválido. Solicita uno nuevo.'
})

async function submit() {
  error.value = ''
  if (newPassword.value.length < 8) {
    error.value = 'La contraseña debe tener al menos 8 caracteres.'
    return
  }
  if (newPassword.value !== confirm.value) {
    error.value = 'Las contraseñas no coinciden.'
    return
  }
  loading.value = true
  try {
    await adminResetPassword(token.value, newPassword.value)
    done.value = true
    setTimeout(() => router.push('/admin/login'), 2500)
  } catch (e: any) {
    error.value = e?.response?.data?.title ?? 'Error al restablecer. Solicita un nuevo enlace.'
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
      <div class="bg-white/5 border border-white/10 rounded-xl p-8 space-y-4">
        <template v-if="done">
          <div class="text-center py-4">
            <p class="text-white font-bold mb-2">¡Contraseña actualizada!</p>
            <p class="text-white/50 text-sm">Redirigiendo al login...</p>
          </div>
        </template>
        <template v-else>
          <div>
            <h2 class="text-white font-bold mb-1">Nueva contraseña</h2>
          </div>
          <form @submit.prevent="submit" class="space-y-4">
            <div>
              <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-2">Nueva contraseña</label>
              <input
                v-model="newPassword"
                type="password"
                placeholder="Mínimo 8 caracteres"
                required
                class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-3 text-white placeholder-white/30 focus:outline-none focus:border-[#c2f33d] transition-colors"
              />
            </div>
            <div>
              <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-2">Confirmar contraseña</label>
              <input
                v-model="confirm"
                type="password"
                placeholder="Repite la contraseña"
                required
                class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-3 text-white placeholder-white/30 focus:outline-none focus:border-[#c2f33d] transition-colors"
              />
            </div>
            <p v-if="error" class="text-red-400 text-sm">{{ error }}</p>
            <button
              type="submit"
              :disabled="loading || !token"
              class="w-full bg-[#c2f33d] text-[#231f20] font-black py-3 rounded-lg hover:bg-[#d4ff4a] transition-colors disabled:opacity-50"
            >
              {{ loading ? 'Guardando...' : 'Guardar contraseña' }}
            </button>
          </form>
        </template>
        <div class="text-center">
          <RouterLink to="/admin/login" class="text-xs text-white/40 hover:text-white/70 transition-colors">
            Volver al login
          </RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>
