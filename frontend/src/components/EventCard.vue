<script setup lang="ts">
import type { Event } from '../types'

defineProps<{
  event: Event
  onRegister?: () => void
}>()

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', {
    weekday: 'short', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit', timeZone: 'UTC'
  })
}

function daysUntil(iso: string) {
  return Math.ceil((new Date(iso).getTime() - Date.now()) / (1000 * 60 * 60 * 24))
}

function initials(name: string | null) {
  if (!name) return 'DT'
  return name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <div class="border border-gray-200 p-6 flex flex-col gap-4 hover:border-gray-400 transition-colors">

    <!-- Top row: category + speaker avatar (top-right) -->
    <div class="flex items-start justify-between gap-4">
      <div class="flex flex-col gap-1">
        <p class="text-xs font-mono uppercase tracking-widest text-gray-400">
          {{ event.category === 'basics' ? 'Basics' : 'Advanced' }}
        </p>
        <span v-if="!event.isPast && daysUntil(event.scheduledAt) <= 14" class="text-xs font-mono text-[#c2f33d] bg-[#231f20] px-2 py-0.5 self-start">
          In {{ daysUntil(event.scheduledAt) }}d
        </span>
      </div>

      <!-- Speaker: avatar + name, top-right -->
      <div v-if="event.speaker && !event.isPast" class="flex flex-col items-center gap-1.5 flex-shrink-0">
        <div class="w-12 h-12 rounded-full overflow-hidden bg-gray-100">
          <img
            v-if="event.speakerPhotoUrl"
            :src="event.speakerPhotoUrl"
            :alt="event.speaker"
            class="w-full h-full object-cover object-center"
          />
          <div v-else class="w-full h-full bg-[#231f20] flex items-center justify-center">
            <span class="text-[#c2f33d] text-xs font-black select-none">{{ initials(event.speaker) }}</span>
          </div>
        </div>
        <p class="text-xs font-mono text-gray-400 text-center leading-tight max-w-[80px]">{{ event.speaker }}</p>
      </div>
    </div>

    <!-- Title + description -->
    <div class="flex-1">
      <h3 class="text-lg font-black text-[#231f20] leading-snug mb-2">{{ event.title }}</h3>
      <p class="text-sm text-gray-500 leading-relaxed line-clamp-2">{{ event.description }}</p>
    </div>

    <!-- Date -->
    <div class="flex items-center gap-2 text-xs font-mono text-gray-400">
      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="square">
        <rect x="3" y="4" width="18" height="18"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>
      </svg>
      {{ formatDate(event.scheduledAt) }} UTC
    </div>

    <!-- Upcoming -->
    <template v-if="!event.isPast">
      <div class="flex items-center justify-between pt-3 border-t border-gray-100">
        <span class="text-xs text-gray-400 font-mono">{{ event.registrationCount }} registered</span>
        <button
          @click="onRegister?.()"
          class="bg-[#231f20] text-white text-xs font-semibold px-5 py-2 hover:bg-[#3d3839] transition-colors"
        >
          Register →
        </button>
      </div>
    </template>

    <!-- Past / On Demand -->
    <template v-else>
      <div class="flex items-center justify-between pt-3 border-t border-gray-100">
        <span class="text-xs text-gray-400 font-mono">{{ event.registrationCount }} attended</span>
        <a
          v-if="event.recordingUrl"
          :href="event.recordingUrl"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center gap-1.5 border border-gray-300 text-gray-700 text-xs font-semibold px-5 py-2 hover:bg-gray-50 transition-colors"
        >
          <svg width="11" height="11" viewBox="0 0 24 24" fill="currentColor"><path d="M8 5v14l11-7z"/></svg>
          Watch
        </a>
        <span v-else class="text-xs text-gray-400 font-mono italic">Recording soon</span>
      </div>
    </template>
  </div>
</template>
