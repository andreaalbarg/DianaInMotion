<script setup lang="ts">
import { ref } from 'vue'
import { fetchRegistrations, fetchAdminEvents, type Registration, type AdminEvent } from '../../services/adminApi'

const registrations = ref<Registration[]>([])
const events = ref<AdminEvent[]>([])
const selectedEventId = ref('')
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    const [regs, evs] = await Promise.all([
      fetchRegistrations(selectedEventId.value || undefined),
      events.value.length === 0 ? fetchAdminEvents() : Promise.resolve(events.value),
    ])
    registrations.value = regs
    events.value = evs
  } finally {
    loading.value = false
  }
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-US', {
    year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit',
  })
}

load()
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-black">Registrations</h1>
      <select
        v-model="selectedEventId"
        @change="load"
        class="bg-white border border-gray-300 rounded-lg px-4 py-2 text-[#231f20] text-sm focus:outline-none focus:border-[#c2f33d] transition-colors"
      >
        <option value="">All events</option>
        <option v-for="ev in events" :key="ev.id" :value="ev.id">{{ ev.title }}</option>
      </select>
    </div>

    <div v-if="loading" class="text-gray-400">Loading…</div>

    <div v-else class="bg-white border border-gray-200 rounded-xl overflow-hidden shadow-sm">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-gray-200 text-gray-400 text-xs uppercase tracking-wider bg-gray-50">
            <th class="text-left px-5 py-3">Name</th>
            <th class="text-left px-5 py-3">Email</th>
            <th class="text-left px-5 py-3">Event</th>
            <th class="text-left px-5 py-3">Registered</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="r in registrations"
            :key="r.id"
            class="border-b border-gray-100 hover:bg-gray-50 transition-colors"
          >
            <td class="px-5 py-3 font-medium text-[#231f20]">{{ r.name }}</td>
            <td class="px-5 py-3 text-gray-500">{{ r.email }}</td>
            <td class="px-5 py-3 text-gray-500">{{ r.eventTitle }}</td>
            <td class="px-5 py-3 text-gray-400 text-xs">{{ formatDate(r.registeredAt) }}</td>
          </tr>
        </tbody>
      </table>
      <div v-if="registrations.length === 0" class="px-5 py-8 text-center text-gray-400">No registrations.</div>
    </div>

    <div class="mt-3 text-sm text-gray-400">{{ registrations.length }} registrations</div>
  </div>
</template>
