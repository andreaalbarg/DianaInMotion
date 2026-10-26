<script setup lang="ts">
import { ref } from 'vue'
import { adminForgotPassword } from '../../services/adminApi'

const email = ref('')
const loading = ref(false)
const sent = ref(false)
const error = ref('')

async function submit() {
  error.value = ''
  loading.value = true
  try {
    await adminForgotPassword(email.value)
    sent.value = true
  } catch {
    error.value = 'Error al procesar la solicitud. Intenta de nuevo.'
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
        <template v-if="!sent">
          <div>
            <h2 class="text-white font-bold mb-1">Recuperar contraseña</h2>
            <p class="text-white/40 text-sm">Te enviaremos un enlace para crear una nueva contraseña.</p>
          </div>
          <form @submit.prevent="submit" class="space-y-4">
            <div>
              <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-2">Correo</label>
              <input
                v-model="email"
                type="email"
                placeholder="tu@correo.com"
                required
                class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-3 text-white placeholder-white/30 focus:outline-none focus:border-[#c2f33d] transition-colors"
              />
            </div>
            <p v-if="error" class="text-red-400 text-sm">{{ error }}</p>
            <button
              type="submit"
              :disabled="loading"
              class="w-full bg-[#c2f33d] text-[#231f20] font-black py-3 rounded-lg hover:bg-[#d4ff4a] transition-colors disabled:opacity-50"
            >
              {{ loading ? 'Enviando...' : 'Enviar enlace' }}
            </button>
          </form>
        </template>
        <template v-else>
          <div class="text-center py-4">
            <p class="text-white font-bold mb-2">Revisa tu correo</p>
            <p class="text-white/50 text-sm">Si ese correo está registrado, recibirás un enlace en los próximos minutos (incluyendo spam).</p>
          </div>
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
