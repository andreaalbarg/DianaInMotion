<script setup lang="ts">
import { ref, onMounted } from 'vue'
import {
  fetchYouTubeSettings,
  saveYouTubeSettings,
  testYouTubeConnection,
  type YouTubeVideoPreview,
} from '../../services/adminApi'

const apiKey = ref('')
const channelId = ref('')
const saving = ref(false)
const saveError = ref('')
const saveOk = ref(false)

const previewing = ref(false)
const previewError = ref('')
const previewOk = ref(false)
const videos = ref<YouTubeVideoPreview[]>([])

onMounted(async () => {
  try {
    const s = await fetchYouTubeSettings()
    apiKey.value = s.apiKey
    channelId.value = s.channelId
  } catch {}
})

async function save() {
  saving.value = true
  saveError.value = ''
  saveOk.value = false
  try {
    await saveYouTubeSettings({ apiKey: apiKey.value, channelId: channelId.value })
    saveOk.value = true
    setTimeout(() => (saveOk.value = false), 3000)
  } catch (e: any) {
    saveError.value = e?.response?.data?.title ?? e?.message ?? 'Error al guardar.'
  } finally {
    saving.value = false
  }
}

async function testConnection() {
  previewing.value = true
  previewError.value = ''
  previewOk.value = false
  videos.value = []
  try {
    videos.value = await testYouTubeConnection({ apiKey: apiKey.value, channelId: channelId.value })
    previewOk.value = true
  } catch (e: any) {
    previewError.value = e?.response?.data?.title ?? e?.message ?? 'Error al conectar con YouTube.'
  } finally {
    previewing.value = false
  }
}
</script>

<template>
  <div class="max-w-2xl">
    <h1 class="text-2xl font-bold mb-2">Canal de YouTube</h1>
    <p class="text-white/50 text-sm mb-8">
      Configura el canal de YouTube para mostrar automáticamente los últimos 3 videos en la sección
      de Past Sessions de la página.
    </p>

    <div class="bg-white/5 border border-white/10 rounded-xl p-6 space-y-5 mb-6">
      <div>
        <label class="block text-sm font-medium text-white/70 mb-1.5">YouTube API Key</label>
        <input
          v-model="apiKey"
          type="password"
          placeholder="AIza..."
          class="w-full bg-black/30 border border-white/10 rounded-lg px-4 py-2.5 text-sm text-white placeholder:text-white/20 focus:outline-none focus:border-[#c2f33d]/60"
        />
        <p class="text-white/30 text-xs mt-1">
          Crea una API Key en
          <a
            href="https://console.cloud.google.com/apis/credentials"
            target="_blank"
            class="text-[#c2f33d]/70 hover:text-[#c2f33d]"
          >Google Cloud Console</a>
          con acceso a la YouTube Data API v3.
        </p>
      </div>

      <div>
        <label class="block text-sm font-medium text-white/70 mb-1.5">ID del Canal</label>
        <input
          v-model="channelId"
          type="text"
          placeholder="UC... o @handle"
          class="w-full bg-black/30 border border-white/10 rounded-lg px-4 py-2.5 text-sm text-white placeholder:text-white/20 focus:outline-none focus:border-[#c2f33d]/60"
        />
        <p class="text-white/30 text-xs mt-1">
          Ingresa el ID del canal (empieza con "UC") o tu @handle (ej: @DianaInMotion).
        </p>
      </div>

      <div v-if="saveError" class="text-red-400 text-sm bg-red-900/20 rounded-lg px-4 py-2.5">
        {{ saveError }}
      </div>

      <div class="flex gap-3 pt-1">
        <button
          @click="save"
          :disabled="saving"
          class="px-5 py-2.5 bg-[#c2f33d] text-[#231f20] text-sm font-bold rounded-lg hover:bg-[#d4ff4f] disabled:opacity-50 transition-colors"
        >
          {{ saving ? 'Guardando…' : saveOk ? '¡Guardado!' : 'Guardar' }}
        </button>
        <button
          @click="testConnection"
          :disabled="previewing || !apiKey || !channelId"
          class="px-5 py-2.5 bg-white/10 text-white text-sm font-medium rounded-lg hover:bg-white/15 disabled:opacity-40 transition-colors"
        >
          {{ previewing ? 'Probando…' : 'Probar conexión' }}
        </button>
      </div>
    </div>

    <!-- Preview -->
    <div v-if="previewError" class="text-red-400 text-sm bg-red-900/20 rounded-lg px-4 py-3 mb-6">
      {{ previewError }}
    </div>

    <div v-if="previewOk && videos.length === 0" class="text-[#c2f33d]/80 text-sm bg-[#c2f33d]/10 rounded-lg px-4 py-3 mb-6">
      Conexión exitosa. El canal aún no tiene videos públicos — aparecerán automáticamente en Past Sessions en cuanto se publique el primero.
    </div>

    <div v-if="videos.length > 0">
      <h2 class="text-sm font-semibold text-white/50 uppercase tracking-widest mb-4">
        Últimos videos del canal
      </h2>
      <div class="grid grid-cols-1 gap-4">
        <a
          v-for="v in videos"
          :key="v.videoId"
          :href="v.url"
          target="_blank"
          class="flex gap-4 bg-white/5 border border-white/10 rounded-xl p-4 hover:border-white/20 transition-colors"
        >
          <img
            v-if="v.thumbnailUrl"
            :src="v.thumbnailUrl"
            :alt="v.title"
            class="w-32 h-18 object-cover rounded-lg flex-shrink-0"
          />
          <div class="min-w-0">
            <div class="text-sm font-medium text-white line-clamp-2">{{ v.title }}</div>
            <div class="text-xs text-[#c2f33d]/70 mt-1">Ver en YouTube ↗</div>
          </div>
        </a>
      </div>
      <p class="text-white/30 text-xs mt-4">
        Estos videos se mostrarán automáticamente en la sección de Past Sessions (se actualizan cada hora).
      </p>
    </div>
  </div>
</template>
