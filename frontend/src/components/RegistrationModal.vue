<script setup lang="ts">
import { ref } from 'vue'
import { registerForEvent } from '../services/api'
import type { Event } from '../types'

const props = defineProps<{ event: Event }>()
const emit = defineEmits<{ close: []; registered: [] }>()

const name = ref('')
const email = ref('')
const loading = ref(false)
const error = ref('')
const success = ref(false)

async function submit() {
  if (!name.value.trim() || !email.value.trim()) {
    error.value = 'Por favor completa todos los campos.'
    return
  }
  loading.value = true
  error.value = ''
  try {
    await registerForEvent(props.event.id, name.value.trim(), email.value.trim())
    success.value = true
    emit('registered')
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'Ocurrió un error. Intenta de nuevo.'
  } finally {
    loading.value = false
  }
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('es-MX', {
    weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
    hour: '2-digit', minute: '2-digit', timeZone: 'UTC'
  })
}
</script>

<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-black/70 backdrop-blur-sm" @click="emit('close')" />

    <div class="relative w-full max-w-md bg-[#151719] border border-[#2A2D35] rounded-2xl p-6 shadow-2xl">
      <button @click="emit('close')" class="absolute top-4 right-4 text-[#6B7280] hover:text-white transition-colors">
        <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
          <path d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"/>
        </svg>
      </button>

      <template v-if="!success">
        <div class="mb-5">
          <span
            class="text-xs font-semibold px-2 py-1 rounded-full"
            :class="event.category === 'basics' ? 'bg-emerald-900/50 text-emerald-400' : 'bg-purple-900/50 text-purple-400'"
          >
            {{ event.category === 'basics' ? 'In Motion / Basics' : 'In Motion / Advanced' }}
          </span>
          <h2 class="text-white font-bold text-xl mt-3 mb-1">{{ event.title }}</h2>
          <p class="text-[#6B7280] text-sm">{{ formatDate(event.scheduledAt) }}</p>
        </div>

        <form @submit.prevent="submit" class="space-y-4">
          <div>
            <label class="block text-[#9CA3AF] text-sm mb-1.5">Nombre completo</label>
            <input
              v-model="name"
              type="text"
              placeholder="Tu nombre"
              class="w-full bg-[#0B0C0E] border border-[#2A2D35] rounded-lg px-4 py-2.5 text-white placeholder-[#4B5563] text-sm focus:outline-none focus:border-[#A8FF00] transition-colors"
            />
          </div>
          <div>
            <label class="block text-[#9CA3AF] text-sm mb-1.5">Email</label>
            <input
              v-model="email"
              type="email"
              placeholder="tu@email.com"
              class="w-full bg-[#0B0C0E] border border-[#2A2D35] rounded-lg px-4 py-2.5 text-white placeholder-[#4B5563] text-sm focus:outline-none focus:border-[#A8FF00] transition-colors"
            />
          </div>

          <p v-if="error" class="text-red-400 text-sm">{{ error }}</p>

          <button
            type="submit"
            :disabled="loading"
            class="w-full bg-[#A8FF00] text-[#0B0C0E] font-bold py-3 rounded-lg hover:bg-[#BFFF33] transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
          >
            {{ loading ? 'Registrando...' : 'Confirmar registro' }}
          </button>
        </form>
      </template>

      <template v-else>
        <div class="text-center py-6">
          <div class="w-14 h-14 rounded-full bg-[#A8FF00]/10 flex items-center justify-center mx-auto mb-4">
            <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#A8FF00" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="20 6 9 17 4 12"/>
            </svg>
          </div>
          <h3 class="text-white font-bold text-xl mb-2">¡Registro exitoso!</h3>
          <p class="text-[#9CA3AF] text-sm mb-6">Te enviaremos los detalles por email antes del evento.</p>
          <button
            @click="emit('close')"
            class="bg-[#A8FF00] text-[#0B0C0E] font-bold px-6 py-2.5 rounded-lg hover:bg-[#BFFF33] transition-colors"
          >
            Cerrar
          </button>
        </div>
      </template>
    </div>
  </div>
</template>
