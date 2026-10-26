<script setup lang="ts">
import { ref, computed } from 'vue'
import type { Event } from '../types'

const props = defineProps<{
  events: Event[]
  onRegister: (event: Event) => void
}>()

const DAYS = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su']

// Start on the month of the first upcoming event (or current month)
const firstEventDate = props.events.length
  ? new Date(props.events[0].scheduledAt)
  : new Date()

const viewYear = ref(firstEventDate.getUTCFullYear())
const viewMonth = ref(firstEventDate.getUTCMonth())

function prevMonth() {
  if (viewMonth.value === 0) { viewYear.value--; viewMonth.value = 11 }
  else viewMonth.value--
}
function nextMonth() {
  if (viewMonth.value === 11) { viewYear.value++; viewMonth.value = 0 }
  else viewMonth.value++
}

const monthLabel = computed(() =>
  new Date(Date.UTC(viewYear.value, viewMonth.value, 1))
    .toLocaleDateString('en-US', { month: 'long', year: 'numeric', timeZone: 'UTC' })
)

const cells = computed(() => {
  const firstDow = new Date(Date.UTC(viewYear.value, viewMonth.value, 1)).getUTCDay()
  const daysInMonth = new Date(Date.UTC(viewYear.value, viewMonth.value + 1, 0)).getUTCDate()
  const startOffset = (firstDow + 6) % 7
  const total = Math.ceil((startOffset + daysInMonth) / 7) * 7
  return Array.from({ length: total }, (_, i) => {
    const day = i - startOffset + 1
    return day >= 1 && day <= daysInMonth ? day : null
  })
})

function eventForDay(day: number) {
  return props.events.find(e => {
    const d = new Date(e.scheduledAt)
    return d.getUTCFullYear() === viewYear.value
      && d.getUTCMonth() === viewMonth.value
      && d.getUTCDate() === day
  }) ?? null
}

const monthEvents = computed(() =>
  props.events.filter(e => {
    const d = new Date(e.scheduledAt)
    return d.getUTCFullYear() === viewYear.value && d.getUTCMonth() === viewMonth.value
  })
)

function formatTime(iso: string, tz: string) {
  return new Date(iso).toLocaleTimeString('en-US', {
    hour: '2-digit', minute: '2-digit',
    timeZone: tz || 'UTC', timeZoneName: 'short',
  })
}

function initials(name: string) {
  return name.split(' ').map((w: string) => w[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <div class="grid md:grid-cols-[auto_1fr] gap-10 items-start">

    <!-- Calendar grid -->
    <div class="w-full md:w-72 flex-shrink-0">

      <!-- Month nav -->
      <div class="flex items-center justify-between mb-4">
        <button
          @click="prevMonth"
          class="w-8 h-8 flex items-center justify-center border border-gray-200 hover:border-[#231f20] hover:text-[#231f20] text-gray-400 transition-colors"
          aria-label="Previous month"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="square"><polyline points="15 18 9 12 15 6"/></svg>
        </button>
        <span class="text-sm font-black text-[#231f20] tracking-tight">{{ monthLabel }}</span>
        <button
          @click="nextMonth"
          class="w-8 h-8 flex items-center justify-center border border-gray-200 hover:border-[#231f20] hover:text-[#231f20] text-gray-400 transition-colors"
          aria-label="Next month"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="square"><polyline points="9 18 15 12 9 6"/></svg>
        </button>
      </div>

      <!-- Grid -->
      <div class="border border-gray-200">
        <!-- Day headers -->
        <div class="grid grid-cols-7 border-b border-gray-200">
          <div v-for="d in DAYS" :key="d" class="text-center py-2 text-xs font-mono text-gray-400">
            {{ d }}
          </div>
        </div>
        <!-- Day cells -->
        <div class="grid grid-cols-7">
          <template v-for="(day, i) in cells" :key="i">
            <div
              v-if="day"
              class="aspect-square flex items-center justify-center"
              :class="[
                (i % 7) < 6 ? 'border-r border-gray-100' : '',
                Math.floor(i / 7) < Math.floor(cells.length / 7) - 1 ? 'border-b border-gray-100' : ''
              ]"
            >
              <div v-if="eventForDay(day)" class="w-7 h-7 bg-[#c2f33d] flex items-center justify-center">
                <span class="text-[#231f20] text-xs font-black">{{ day }}</span>
              </div>
              <span v-else class="text-xs text-gray-300">{{ day }}</span>
            </div>
            <div v-else class="aspect-square" />
          </template>
        </div>
      </div>
    </div>

    <!-- Event list -->
    <div class="flex flex-col divide-y divide-gray-100">

      <div v-if="monthEvents.length === 0" class="py-12 text-center">
        <p class="text-xs font-mono uppercase tracking-widest text-gray-400">No sessions this month.</p>
      </div>

      <div
        v-for="event in monthEvents"
        :key="event.id"
        class="py-8 flex flex-col sm:flex-row sm:items-start justify-between gap-8"
      >
        <!-- Date block -->
        <div class="flex-shrink-0 text-center w-14">
          <p class="text-4xl font-black text-[#231f20] leading-none">
            {{ new Date(event.scheduledAt).getUTCDate() }}
          </p>
          <p class="text-xs font-mono text-gray-400 uppercase tracking-wide mt-1">
            {{ new Date(event.scheduledAt).toLocaleDateString('en-US', { weekday: 'short', timeZone: 'UTC' }) }}
          </p>
        </div>

        <!-- Event info -->
        <div class="flex-1 min-w-0">
          <p class="text-xs font-mono uppercase tracking-widest text-gray-400 mb-2">
            {{ event.category === 'basics' ? 'Basics' : 'Advanced' }} · {{ formatTime(event.scheduledAt, event.timezone) }}
            <span class="ml-1 px-1.5 py-0.5 border text-[10px]"
              :class="event.language === 'en' ? 'border-blue-200 text-blue-500' : 'border-orange-200 text-orange-500'">
              {{ event.language === 'en' ? 'EN' : 'ES' }}
            </span>
          </p>
          <h4 class="text-xl font-black text-[#231f20] leading-tight mb-2">{{ event.title }}</h4>
          <p class="text-sm text-gray-500 leading-relaxed mb-4">{{ event.description }}</p>

          <div v-if="event.speaker" class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-full overflow-hidden flex-shrink-0">
              <img v-if="event.speakerPhotoUrl" :src="event.speakerPhotoUrl" :alt="event.speaker" class="w-full h-full object-cover" />
              <div v-else class="w-full h-full bg-[#231f20] flex items-center justify-center">
                <span class="text-[#c2f33d] text-[10px] font-black select-none">{{ initials(event.speaker) }}</span>
              </div>
            </div>
            <div class="leading-none">
              <p class="text-xs font-semibold text-[#231f20]">{{ event.speaker }}</p>
              <p class="text-xs text-gray-400 font-mono mt-0.5">{{ event.registrationCount }} registered</p>
            </div>
          </div>
        </div>

        <button
          @click="onRegister(event)"
          class="flex-shrink-0 bg-[#231f20] text-white text-xs font-semibold px-6 py-2.5 hover:bg-[#3d3839] transition-colors self-start"
        >
          Register →
        </button>
      </div>
    </div>

  </div>
</template>
