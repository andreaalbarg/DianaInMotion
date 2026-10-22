<script setup lang="ts">
import type { Event } from '../types'

const props = defineProps<{
  event: Event
  onRegister?: () => void
}>()

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('es-MX', {
    weekday: 'short', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit', timeZone: 'UTC'
  })
}

function daysUntil(iso: string) {
  const diff = new Date(iso).getTime() - Date.now()
  return Math.ceil(diff / (1000 * 60 * 60 * 24))
}
</script>

<template>
  <div class="bg-[#151719] border border-[#2A2D35] rounded-2xl p-5 flex flex-col gap-4 hover:border-[#3A3D48] transition-colors">
    <div class="flex items-start justify-between gap-3">
      <span
        class="text-xs font-semibold px-2.5 py-1 rounded-full flex-shrink-0"
        :class="event.category === 'basics'
          ? 'bg-emerald-900/50 text-emerald-400'
          : 'bg-purple-900/50 text-purple-400'"
      >
        {{ event.category === 'basics' ? 'Basics' : 'Advanced' }}
      </span>

      <span v-if="!event.isPast" class="text-xs text-[#6B7280] flex-shrink-0">
        <template v-if="daysUntil(event.scheduledAt) <= 7">
          <span class="text-[#A8FF00] font-medium">En {{ daysUntil(event.scheduledAt) }} días</span>
        </template>
        <template v-else>
          {{ daysUntil(event.scheduledAt) }} días
        </template>
      </span>
    </div>

    <div class="flex-1">
      <h3 class="text-white font-semibold text-base leading-snug mb-2">{{ event.title }}</h3>
      <p class="text-[#6B7280] text-sm leading-relaxed line-clamp-2">{{ event.description }}</p>
    </div>

    <div class="flex items-center gap-2 text-[#6B7280] text-xs">
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>
      </svg>
      <span>{{ formatDate(event.scheduledAt) }} UTC</span>
    </div>

    <div v-if="event.speaker" class="flex items-center gap-2 text-[#6B7280] text-xs">
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>
      </svg>
      <span>{{ event.speaker }}</span>
    </div>

    <!-- Upcoming: register button -->
    <template v-if="!event.isPast">
      <div class="flex items-center justify-between pt-1 border-t border-[#2A2D35]">
        <span class="text-[#6B7280] text-xs">{{ event.registrationCount }} registrados</span>
        <button
          @click="onRegister?.()"
          class="bg-[#A8FF00] text-[#0B0C0E] text-sm font-bold px-4 py-2 rounded-lg hover:bg-[#BFFF33] transition-colors"
        >
          Registrarme
        </button>
      </div>
    </template>

    <!-- Past: watch recording or coming soon -->
    <template v-else>
      <div class="flex items-center justify-between pt-1 border-t border-[#2A2D35]">
        <span class="text-[#6B7280] text-xs">{{ event.registrationCount }} asistentes</span>
        <a
          v-if="event.recordingUrl"
          :href="event.recordingUrl"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center gap-1.5 bg-[#1E2028] border border-[#2A2D35] text-white text-sm font-medium px-4 py-2 rounded-lg hover:border-[#A8FF00] hover:text-[#A8FF00] transition-colors"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor"><path d="M8 5v14l11-7z"/></svg>
          Ver grabación
        </a>
        <span v-else class="text-[#4B5563] text-xs italic">Grabación próximamente</span>
      </div>
    </template>
  </div>
</template>
