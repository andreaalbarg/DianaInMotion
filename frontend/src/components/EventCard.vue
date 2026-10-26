<script setup lang="ts">
import type { Event } from '../types'

defineProps<{
  event: Event
  onRegister?: () => void
}>()

function formatDate(iso: string, tz: string) {
  return new Date(iso).toLocaleString('en-US', {
    weekday: 'short', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit',
    timeZone: tz || 'UTC', timeZoneName: 'short',
  })
}

function daysUntil(iso: string) {
  return Math.ceil((new Date(iso).getTime() - Date.now()) / (1000 * 60 * 60 * 24))
}

function initials(name: string | null) {
  if (!name) return 'DT'
  return name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase()
}

function youtubeId(url: string | null): string | null {
  if (!url) return null
  try {
    return new URL(url).searchParams.get('v')
  } catch {
    return null
  }
}
</script>

<template>
  <div class="border border-gray-200 flex flex-col hover:border-gray-400 transition-colors overflow-hidden">

    <!-- YouTube thumbnail for past sessions -->
    <a
      v-if="event.isPast && youtubeId(event.recordingUrl)"
      :href="event.recordingUrl!"
      target="_blank"
      rel="noopener noreferrer"
      class="relative block overflow-hidden group"
    >
      <img
        :src="`https://img.youtube.com/vi/${youtubeId(event.recordingUrl)}/hqdefault.jpg`"
        :alt="event.title"
        class="w-full aspect-video object-cover"
      />
      <div class="absolute inset-0 bg-black/30 flex items-center justify-center group-hover:bg-black/10 transition-colors">
        <div class="w-14 h-14 bg-[#c2f33d] flex items-center justify-center group-hover:scale-105 transition-transform">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="#231f20"><path d="M8 5v14l11-7z"/></svg>
        </div>
      </div>
    </a>

    <!-- Card body -->
    <div class="p-6 flex flex-col gap-4 flex-1">

      <!-- Category + Language -->
      <div class="flex items-center justify-between gap-2">
        <div class="flex items-center gap-2">
          <p class="text-xs font-mono uppercase tracking-widest text-gray-400">
            {{ event.category === 'basics' ? 'Basics' : 'Advanced' }}
          </p>
          <span class="text-xs font-mono uppercase tracking-widest px-2 py-0.5 border"
            :class="event.language === 'en'
              ? 'border-blue-200 text-blue-500'
              : 'border-orange-200 text-orange-500'">
            {{ event.language === 'en' ? 'EN' : 'ES' }}
          </span>
        </div>
        <span v-if="!event.isPast && daysUntil(event.scheduledAt) <= 14" class="text-xs font-mono text-[#c2f33d] bg-[#231f20] px-2 py-0.5">
          In {{ daysUntil(event.scheduledAt) }}d
        </span>
      </div>

      <!-- Speaker (upcoming only) -->
      <div v-if="event.speaker && !event.isPast" class="flex items-center gap-3">
        <div class="w-10 h-10 rounded-full overflow-hidden flex-shrink-0 bg-gray-100">
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
        <p class="text-xs font-mono text-gray-400">{{ event.speaker }}</p>
      </div>

      <!-- Title + description -->
      <div class="flex-1">
        <h3 class="text-lg font-black text-[#231f20] leading-snug mb-2">{{ event.title }}</h3>
        <p class="text-sm text-gray-500 leading-relaxed line-clamp-2">{{ event.description }}</p>
      </div>

      <!-- Date (upcoming only) -->
      <div v-if="!event.isPast" class="flex items-center gap-2 text-xs font-mono text-gray-400">
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="square">
          <rect x="3" y="4" width="18" height="18"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>
        </svg>
        {{ formatDate(event.scheduledAt, event.timezone) }}
      </div>

      <!-- Upcoming footer -->
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

    </div>
  </div>
</template>
