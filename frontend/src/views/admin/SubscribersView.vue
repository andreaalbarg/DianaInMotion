<script setup lang="ts">
import { ref } from 'vue'
import { fetchSubscribers, deleteSubscriber, type Subscriber } from '../../services/adminApi'

const subscribers = ref<Subscriber[]>([])
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    subscribers.value = await fetchSubscribers()
  } finally {
    loading.value = false
  }
}

async function remove(id: string, email: string) {
  if (!confirm(`Remove ${email} from subscribers?`)) return
  await deleteSubscriber(id)
  await load()
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('en-US', {
    year: 'numeric', month: 'short', day: 'numeric',
  })
}

load()
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-black">Newsletter Subscribers</h1>
    </div>

    <div v-if="loading" class="text-gray-400">Loading…</div>

    <div v-else class="bg-white border border-gray-200 rounded-xl overflow-hidden shadow-sm">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-gray-200 text-gray-400 text-xs uppercase tracking-wider bg-gray-50">
            <th class="text-left px-5 py-3">Email</th>
            <th class="text-left px-5 py-3">Subscribed</th>
            <th class="text-right px-5 py-3"></th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="s in subscribers"
            :key="s.id"
            class="border-b border-gray-100 hover:bg-gray-50 transition-colors"
          >
            <td class="px-5 py-3 font-medium text-[#231f20]">{{ s.email }}</td>
            <td class="px-5 py-3 text-gray-400 text-xs">{{ formatDate(s.subscribedAt) }}</td>
            <td class="px-5 py-3 text-right">
              <button @click="remove(s.id, s.email)" class="text-red-400/60 hover:text-red-500 transition-colors text-xs">Remove</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="subscribers.length === 0" class="px-5 py-8 text-center text-gray-400">No subscribers yet.</div>
    </div>

    <div class="mt-3 text-sm text-gray-400">{{ subscribers.length }} subscribers</div>
  </div>
</template>
