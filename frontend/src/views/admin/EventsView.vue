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
const loading = ref(true)
const featuredCount = computed(() => events.value.filter(e => e.isPast && e.isFeatured).length)
const showForm = ref(false)
const editingId = ref<string | null>(null)
const error = ref('')

const emptyForm = (): EventWriteRequest => ({
  title: '',
  description: '',
  category: 'basics',
  scheduledAt: '',
  speaker: '',
  speakerPhotoUrl: '',
  recordingUrl: '',
})

const form = ref<EventWriteRequest>(emptyForm())

async function load() {
  loading.value = true
  try {
    events.value = await fetchAdminEvents()
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
    speakerPhotoUrl: ev.speakerPhotoUrl ?? '',
    recordingUrl: ev.recordingUrl ?? '',
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

onMounted(load)
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-black">Events</h1>
        <p class="text-sm text-white/40 mt-0.5">
          <span :class="featuredCount >= 3 ? 'text-[#c2f33d]' : 'text-white/40'">{{ featuredCount }}/3</span>
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

    <div v-if="loading" class="text-white/40">Loading…</div>

    <div v-else class="bg-white/5 border border-white/10 rounded-xl overflow-hidden">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-white/10 text-white/40 text-xs uppercase tracking-wider">
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
            class="border-b border-white/5 hover:bg-white/5 transition-colors"
          >
            <td class="px-5 py-3 font-medium">
              <span class="inline-block w-2 h-2 rounded-full mr-2" :class="ev.isPast ? 'bg-white/20' : 'bg-[#c2f33d]'"></span>
              {{ ev.title }}
            </td>
            <td class="px-5 py-3 text-white/60 capitalize">{{ ev.category }}</td>
            <td class="px-5 py-3 text-white/60">{{ formatDate(ev.scheduledAt) }}</td>
            <td class="px-5 py-3 text-center">
              <button
                v-if="ev.isPast"
                @click="toggleFeatured(ev)"
                :title="ev.isFeatured ? 'Quitar del landing' : 'Destacar en landing'"
                class="text-xl transition-all"
                :class="ev.isFeatured ? 'text-[#c2f33d]' : 'text-white/20 hover:text-white/60'"
              >★</button>
              <span v-else class="text-white/10 text-xs">—</span>
            </td>
            <td class="px-5 py-3 text-right text-white/60">{{ ev.registrationCount }}</td>
            <td class="px-5 py-3 text-right">
              <button @click="openEdit(ev)" class="text-white/40 hover:text-white mr-3 transition-colors">Edit</button>
              <button @click="remove(ev.id, ev.title)" class="text-red-400/60 hover:text-red-400 transition-colors">Delete</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="events.length === 0" class="px-5 py-8 text-center text-white/40">No events yet.</div>
    </div>

    <!-- Form modal -->
    <div v-if="showForm" class="fixed inset-0 bg-black/70 flex items-center justify-center z-50 p-4">
      <div class="bg-[#2a2527] border border-white/10 rounded-xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
        <div class="flex items-center justify-between p-6 border-b border-white/10">
          <h2 class="font-black text-lg">{{ editingId ? 'Edit Event' : 'New Event' }}</h2>
          <button @click="showForm = false" class="text-white/40 hover:text-white transition-colors text-xl leading-none">&times;</button>
        </div>
        <form @submit.prevent="submit" class="p-6 space-y-4">
          <div>
            <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-1.5">Title *</label>
            <input v-model="form.title" required class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-2.5 text-white placeholder-white/30 focus:outline-none focus:border-[#c2f33d] transition-colors" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-1.5">Description *</label>
            <textarea v-model="form.description" required rows="3" class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-2.5 text-white placeholder-white/30 focus:outline-none focus:border-[#c2f33d] transition-colors resize-none"></textarea>
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-1.5">Category *</label>
              <select v-model="form.category" class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-[#c2f33d] transition-colors">
                <option value="basics">Basics</option>
                <option value="advanced">Advanced</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-1.5">Date & Time *</label>
              <input v-model="form.scheduledAt" type="datetime-local" required class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-[#c2f33d] transition-colors" />
            </div>
          </div>
          <div>
            <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-1.5">Speaker</label>
            <input v-model="form.speaker" placeholder="Speaker name" class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-2.5 text-white placeholder-white/30 focus:outline-none focus:border-[#c2f33d] transition-colors" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-1.5">Speaker Photo URL</label>
            <input v-model="form.speakerPhotoUrl" placeholder="https://..." class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-2.5 text-white placeholder-white/30 focus:outline-none focus:border-[#c2f33d] transition-colors" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-white/60 uppercase tracking-widest mb-1.5">YouTube Recording URL</label>
            <input v-model="form.recordingUrl" placeholder="https://youtube.com/watch?v=..." class="w-full bg-white/10 border border-white/20 rounded-lg px-4 py-2.5 text-white placeholder-white/30 focus:outline-none focus:border-[#c2f33d] transition-colors" />
          </div>
          <p v-if="error" class="text-red-400 text-sm">{{ error }}</p>
          <div class="flex gap-3 pt-2">
            <button type="submit" class="flex-1 bg-[#c2f33d] text-[#231f20] font-black py-3 rounded-lg hover:bg-[#d4ff4a] transition-colors">
              {{ editingId ? 'Save Changes' : 'Create Event' }}
            </button>
            <button type="button" @click="showForm = false" class="px-6 border border-white/20 rounded-lg text-white/60 hover:text-white hover:border-white/40 transition-colors">
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
