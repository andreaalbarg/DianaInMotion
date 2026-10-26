<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  fetchAdminEvents,
  createEvent,
  updateEvent,
  deleteEvent,
  setFeatured,
  type AdminEvent,
  type EventWriteRequest,
} from '../../services/adminApi'

const events = ref<AdminEvent[]>([])
const loading = ref(false)
const loadError = ref('')
const featuredCount = computed(() => events.value.filter(e => e.isPast && e.isFeatured).length)
const showForm = ref(false)
const editingId = ref<string | null>(null)
const error = ref('')

const emptyForm = (): EventWriteRequest => ({
  title: '',
  description: '',
  category: 'basics',
  language: 'es',
  scheduledAt: '',
  speaker: '',
  speakerPhotoUrl: '',
  recordingUrl: '',
  meetingUrl: '',
})

const form = ref<EventWriteRequest>(emptyForm())

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    events.value = await fetchAdminEvents()
  } catch (e: any) {
    const status = e?.response?.status
    if (status === 401) {
      loadError.value = 'Sesión expirada. Inicia sesión de nuevo.'
    } else {
      loadError.value = `Error${status ? ` ${status}` : ''}: ${e?.response?.data?.title ?? e?.message ?? 'No se pudo conectar con el servidor.'}`
    }
  } finally {
    loading.value = false
  }
}

function openCreate() {
  form.value = emptyForm()
  editingId.value = null
  showForm.value = true
  error.value = ''
}

function openEdit(ev: AdminEvent) {
  form.value = {
    title: ev.title,
    description: ev.description,
    category: ev.category,
    scheduledAt: ev.scheduledAt.slice(0, 16),
    speaker: ev.speaker ?? '',
    language: ev.language ?? 'es',
    speakerPhotoUrl: ev.speakerPhotoUrl ?? '',
    recordingUrl: ev.recordingUrl ?? '',
    meetingUrl: ev.meetingUrl ?? '',
  }
  editingId.value = ev.id
  showForm.value = true
  error.value = ''
}

async function submit() {
  error.value = ''
  const req: EventWriteRequest = {
    ...form.value,
    scheduledAt: new Date(form.value.scheduledAt).toISOString(),
    speaker: form.value.speaker || undefined,
    speakerPhotoUrl: form.value.speakerPhotoUrl || undefined,
    recordingUrl: form.value.recordingUrl || undefined,
    meetingUrl: form.value.meetingUrl || undefined,
  }
  try {
    if (editingId.value) {
      await updateEvent(editingId.value, req)
    } else {
      await createEvent(req)
    }
    showForm.value = false
    await load()
  } catch {
    error.value = 'Failed to save event.'
  }
}

async function toggleFeatured(ev: AdminEvent) {
  try {
    await setFeatured(ev.id, !ev.isFeatured)
    await load()
  } catch (e: any) {
    alert(e?.response?.data?.title ?? 'No se pudo actualizar.')
  }
}

async function remove(id: string, title: string) {
  if (!confirm(`Delete "${title}"?`)) return
  await deleteEvent(id)
  await load()
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-US', {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

load()
onMounted(() => { if (!loading.value && events.value.length === 0 && !loadError.value) load() })
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-black">Events</h1>
        <p class="text-sm text-gray-400 mt-0.5">
          <span :class="featuredCount >= 3 ? 'text-[#c2f33d]' : 'text-gray-400'">{{ featuredCount }}/3</span>
          past sessions destacadas en el landing
        </p>
      </div>
      <button
        @click="openCreate"
        class="bg-[#c2f33d] text-[#231f20] font-bold px-5 py-2 rounded-lg hover:bg-[#d4ff4a] transition-colors text-sm"
      >
        + New Event
      </button>
    </div>

    <div v-if="loading" class="text-gray-400">Loading…</div>

    <div v-else-if="loadError" class="bg-red-50 border border-red-200 rounded-xl p-6 text-red-500 text-sm space-y-3">
      <p>{{ loadError }}</p>
      <div class="flex gap-3">
        <button @click="load" class="px-4 py-1.5 bg-gray-100 text-[#231f20] rounded-lg text-xs hover:bg-gray-200">Reintentar</button>
        <RouterLink to="/admin/login" class="px-4 py-1.5 bg-gray-100 text-[#231f20] rounded-lg text-xs hover:bg-gray-200">Ir a login</RouterLink>
      </div>
    </div>

    <div v-else class="bg-white border border-gray-200 rounded-xl overflow-hidden shadow-sm">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-gray-200 text-gray-400 text-xs uppercase tracking-wider bg-gray-50">
            <th class="text-left px-5 py-3">Title</th>
            <th class="text-left px-5 py-3">Category</th>
            <th class="text-left px-5 py-3">Date</th>
            <th class="text-center px-5 py-3">Featured</th>
            <th class="text-right px-5 py-3">Registrations</th>
            <th class="text-right px-5 py-3"></th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="ev in events"
            :key="ev.id"
            class="border-b border-gray-100 hover:bg-gray-50 transition-colors"
          >
            <td class="px-5 py-3 font-medium text-[#231f20]">
              <span class="inline-block w-2 h-2 rounded-full mr-2" :class="ev.isPast ? 'bg-gray-300' : 'bg-[#c2f33d]'"></span>
              {{ ev.title }}
            </td>
            <td class="px-5 py-3 text-gray-500 capitalize">{{ ev.category }}</td>
            <td class="px-5 py-3 text-gray-500">{{ formatDate(ev.scheduledAt) }}</td>
            <td class="px-5 py-3 text-center">
              <button
                v-if="ev.isPast"
                @click="toggleFeatured(ev)"
                :title="ev.isFeatured ? 'Quitar del landing' : 'Destacar en landing'"
                class="text-xl transition-all"
                :class="ev.isFeatured ? 'text-[#c2f33d]' : 'text-gray-300 hover:text-gray-400'"
              >★</button>
              <span v-else class="text-gray-200 text-xs">—</span>
            </td>
            <td class="px-5 py-3 text-right text-gray-500">{{ ev.registrationCount }}</td>
            <td class="px-5 py-3 text-right">
              <button @click="openEdit(ev)" class="text-gray-400 hover:text-[#231f20] mr-3 transition-colors">Edit</button>
              <button @click="remove(ev.id, ev.title)" class="text-red-400/60 hover:text-red-500 transition-colors">Delete</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="events.length === 0" class="px-5 py-8 text-center text-gray-400">No events yet.</div>
    </div>

    <!-- Form modal -->
    <div v-if="showForm" class="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
      <div class="bg-white border border-gray-200 rounded-xl w-full max-w-lg max-h-[90vh] overflow-y-auto shadow-xl">
        <div class="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 class="font-black text-lg text-[#231f20]">{{ editingId ? 'Edit Event' : 'New Event' }}</h2>
          <button @click="showForm = false" class="text-gray-400 hover:text-[#231f20] transition-colors text-xl leading-none">&times;</button>
        </div>
        <form @submit.prevent="submit" class="p-6 space-y-4">
          <div>
            <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Title *</label>
            <input v-model="form.title" required class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] placeholder-gray-400 focus:outline-none focus:border-[#c2f33d] transition-colors" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Description *</label>
            <textarea v-model="form.description" required rows="3" class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] placeholder-gray-400 focus:outline-none focus:border-[#c2f33d] transition-colors resize-none"></textarea>
          </div>
          <div class="grid grid-cols-3 gap-4">
            <div>
              <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Category *</label>
              <select v-model="form.category" class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] focus:outline-none focus:border-[#c2f33d] transition-colors">
                <option value="basics">Basics</option>
                <option value="advanced">Advanced</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Idioma *</label>
              <select v-model="form.language" class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] focus:outline-none focus:border-[#c2f33d] transition-colors">
                <option value="es">Español</option>
                <option value="en">English</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Date & Time *</label>
              <input v-model="form.scheduledAt" type="datetime-local" required class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] focus:outline-none focus:border-[#c2f33d] transition-colors" />
            </div>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Speaker</label>
            <input v-model="form.speaker" placeholder="Speaker name" class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] placeholder-gray-400 focus:outline-none focus:border-[#c2f33d] transition-colors" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Speaker Photo URL</label>
            <input v-model="form.speakerPhotoUrl" placeholder="https://..." class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] placeholder-gray-400 focus:outline-none focus:border-[#c2f33d] transition-colors" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">YouTube Recording URL</label>
            <input v-model="form.recordingUrl" placeholder="https://youtube.com/watch?v=..." class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] placeholder-gray-400 focus:outline-none focus:border-[#c2f33d] transition-colors" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 uppercase tracking-widest mb-1.5">Liga de acceso (Zoom / Meet)</label>
            <input v-model="form.meetingUrl" placeholder="https://zoom.us/j/..." class="w-full bg-gray-50 border border-gray-300 rounded-lg px-4 py-2.5 text-[#231f20] placeholder-gray-400 focus:outline-none focus:border-[#c2f33d] transition-colors" />
          </div>
          <p v-if="error" class="text-red-500 text-sm">{{ error }}</p>
          <div class="flex gap-3 pt-2">
            <button type="submit" class="flex-1 bg-[#c2f33d] text-[#231f20] font-black py-3 rounded-lg hover:bg-[#d4ff4a] transition-colors">
              {{ editingId ? 'Save Changes' : 'Create Event' }}
            </button>
            <button type="button" @click="showForm = false" class="px-6 border border-gray-300 rounded-lg text-gray-500 hover:text-[#231f20] hover:border-gray-400 transition-colors">
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
