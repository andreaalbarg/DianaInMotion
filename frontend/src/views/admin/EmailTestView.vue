<script setup lang="ts">
import { ref } from 'vue'
import { adminApi } from '../../services/adminApi'

const to = ref('')
const name = ref('')
const language = ref('es')
const sending = ref(false)
const result = ref<{ ok: boolean; message: string } | null>(null)

async function send() {
  sending.value = true
  result.value = null
  try {
    const { data } = await adminApi.post<{ message: string }>('/email-test', {
      to: to.value,
      name: name.value || to.value,
      language: language.value,
    }, { timeout: 35000 })
    result.value = { ok: true, message: data.message }
  } catch (e: any) {
    result.value = {
      ok: false,
      message: e?.response?.data?.title ?? e?.message ?? 'Error al enviar.',
    }
  } finally {
    sending.value = false
  }
}
</script>

<template>
  <div class="max-w-md">
    <h1 class="text-2xl font-black mb-1">Email Test</h1>
    <p class="text-sm text-gray-400 mb-6">Manda un correo de confirmación de prueba para verificar que el SMTP está configurado correctamente.</p>

    <div class="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-4">
      <div>
        <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Correo destino *</label>
        <input
          v-model="to"
          type="email"
          placeholder="tu@correo.com"
          required
          class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] placeholder-gray-400 focus:outline-none focus:border-[#c2f33d] transition-colors"
        />
      </div>
      <div>
        <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Nombre</label>
        <input
          v-model="name"
          placeholder="María (opcional)"
          class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] placeholder-gray-400 focus:outline-none focus:border-[#c2f33d] transition-colors"
        />
      </div>
      <div>
        <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Idioma</label>
        <select
          v-model="language"
          class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] focus:outline-none focus:border-[#c2f33d] transition-colors"
        >
          <option value="es">Español</option>
          <option value="en">English</option>
        </select>
      </div>

      <button
        @click="send"
        :disabled="!to || sending"
        class="w-full bg-[#c2f33d] text-[#231f20] font-black py-3 rounded-lg hover:bg-[#d4ff4a] transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
      >
        {{ sending ? 'Conectando al servidor de correo…' : 'Enviar correo de prueba' }}
      </button>

      <div v-if="result" class="rounded-lg px-4 py-3 text-sm font-medium"
        :class="result.ok ? 'bg-green-50 text-green-700 border border-green-200' : 'bg-red-50 text-red-600 border border-red-200'"
      >
        {{ result.message }}
      </div>

      <p v-if="!result" class="text-xs text-gray-400">
        Añade la variable <code class="bg-gray-100 px-1 rounded">MailerDash__ApiKey</code> al entorno del backend con tu API key de MailerDash.
      </p>
    </div>
  </div>
</template>
