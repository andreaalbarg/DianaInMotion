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
    error.value = 'Please fill in all fields.'
    return
  }
  loading.value = true
  error.value = ''
  try {
    await registerForEvent(props.event.id, name.value.trim(), email.value.trim())
    success.value = true
    emit('registered')
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'Something went wrong. Please try again.'
  } finally {
    loading.value = false
  }
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', {
    weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
    hour: '2-digit', minute: '2-digit', timeZone: 'UTC'
  }) + ' UTC'
}
</script>

<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-black/50" @click="emit('close')" />

    <div class="relative w-full max-w-md bg-white border border-gray-200 p-8 shadow-xl">
      <button @click="emit('close')" class="absolute top-4 right-4 text-gray-400 hover:text-gray-900 transition-colors">
        <svg width="18" height="18" viewBox="0 0 20 20" fill="currentColor">
          <path d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"/>
        </svg>
      </button>

      <template v-if="!success">
        <div class="mb-6">
          <p class="text-xs font-mono uppercase tracking-widest text-gray-400 mb-3">
            {{ event.category === 'basics' ? 'In Motion / Basics' : 'In Motion / Advanced' }}
          </p>
          <h2 class="text-2xl font-black text-gray-900 leading-tight mb-2">{{ event.title }}</h2>
          <p class="text-sm text-gray-500 font-mono">{{ formatDate(event.scheduledAt) }}</p>
        </div>

        <form @submit.prevent="submit" class="space-y-4">
          <div>
            <label class="block text-xs font-mono uppercase tracking-widest text-gray-400 mb-2">Full name</label>
            <input
              v-model="name"
              type="text"
              placeholder="Your name"
              class="w-full border border-gray-300 px-4 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:border-gray-900 transition-colors"
            />
          </div>
          <div>
            <label class="block text-xs font-mono uppercase tracking-widest text-gray-400 mb-2">Email</label>
            <input
              v-model="email"
              type="email"
              placeholder="you@example.com"
              class="w-full border border-gray-300 px-4 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:border-gray-900 transition-colors"
            />
          </div>

          <p v-if="error" class="text-red-500 text-xs font-mono">{{ error }}</p>

          <button
            type="submit"
            :disabled="loading"
            class="w-full bg-gray-900 text-white font-semibold py-3 text-sm hover:bg-gray-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ loading ? 'Registering...' : 'Confirm registration' }}
          </button>
        </form>
      </template>

      <template v-else>
        <div class="text-center py-6">
          <div class="w-12 h-12 bg-[#A8FF00] flex items-center justify-center mx-auto mb-5">
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#111" stroke-width="2.5" stroke-linecap="square" stroke-linejoin="miter">
              <polyline points="20 6 9 17 4 12"/>
            </svg>
          </div>
          <h3 class="text-2xl font-black text-gray-900 mb-2">You're registered!</h3>
          <p class="text-sm text-gray-500 mb-6">We'll send you the access link before the session.</p>
          <button
            @click="emit('close')"
            class="bg-gray-900 text-white font-semibold px-8 py-2.5 text-sm hover:bg-gray-700 transition-colors"
          >
            Close
          </button>
        </div>
      </template>
    </div>
  </div>
</template>
