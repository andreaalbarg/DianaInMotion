<script setup lang="ts">
import { computed } from 'vue'
import type { Event } from '../types'

const props = defineProps<{
  events: Event[]
  onRegister: (event: Event) => void
}>()

const DAYS = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su']

// Group events by year-month, sorted chronologically
const months = computed(() => {
  const map = new Map<string, Event[]>()
  for (const e of props.events) {
    const d = new Date(e.scheduledAt)
    const key = `${d.getUTCFullYear()}-${String(d.getUTCMonth()).padStart(2, '0')}`
    if (!map.has(key)) map.set(key, [])
    map.get(key)!.push(e)
  }
  return Array.from(map.entries())
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([key, events]) => {
      const [year, month] = key.split('-').map(Number)
      return { year, month, events }
    })
})

function calendarCells(year: number, month: number) {
  const firstDow = new Date(Date.UTC(year, month, 1)).getUTCDay() // 0=Sun
  const daysInMonth = new Date(Date.UTC(year, month + 1, 0)).getUTCDate()
  const startOffset = (firstDow + 6) % 7 // shift to Mon=0
  const total = Math.ceil((startOffset + daysInMonth) / 7) * 7
  return Array.from({ length: total }, (_, i) => {
    const day = i - startOffset + 1
    return day >= 1 && day <= daysInMonth ? day : null
  })
}

function eventForDay(year: number, month: number, day: number) {
  return props.events.find(e => {
    const d = new Date(e.scheduledAt)
    return d.getUTCFullYear() === year && d.getUTCMonth() === month && d.getUTCDate() === day
  }) ?? null
}

function formatMonthYear(year: number, month: number) {
  return new Date(Date.UTC(year, month, 1)).toLocaleDateString('en-US', {
    month: 'long', year: 'numeric', timeZone: 'UTC'
  })
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-US', {
    hour: '2-digit', minute: '2-digit', timeZone: 'UTC'
  })
}

</script>

<template>
  <div class="space-y-10">
    <div v-for="{ year, month, events: monthEvents } in months" :key="`${year}-${month}`">

      <!-- Month heading -->
      <h3 class="text-xs font-mono uppercase tracking-widest text-gray-400 mb-5">
        {{ formatMonthYear(year, month) }}
      </h3>

      <div class="grid md:grid-cols-[auto_1fr] gap-8 items-start">

        <!-- Calendar grid -->
        <div class="w-full md:w-72 border border-gray-200">
          <!-- Day headers -->
          <div class="grid grid-cols-7 border-b border-gray-200">
            <div
              v-for="d in DAYS" :key="d"
              class="text-center py-2 text-xs font-mono text-gray-400"
            >
              {{ d }}
            </div>
          </div>
          <!-- Day cells -->
          <div class="grid grid-cols-7">
            <template v-for="(day, i) in calendarCells(year, month)" :key="i">
              <div
                v-if="day"
                class="aspect-square flex items-center justify-center relative"
                :class="[
                  (i % 7) < 6 ? 'border-r border-gray-100' : '',
                  Math.floor(i / 7) < Math.floor(calendarCells(year, month).length / 7) - 1 ? 'border-b border-gray-100' : ''
                ]"
              >
                <!-- Event day -->
                <template v-if="eventForDay(year, month, day)">
                  <div class="w-7 h-7 bg-[#231f20] flex items-center justify-center">
                    <span class="text-[#c2f33d] text-xs font-black">{{ day }}</span>
                  </div>
                </template>
                <!-- Regular day -->
                <template v-else>
                  <span class="text-xs text-gray-300">{{ day }}</span>
                </template>
              </div>
              <div v-else class="aspect-square" />
            </template>
          </div>
        </div>

        <!-- Event list for the month -->
        <div class="flex flex-col divide-y divide-gray-100">
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

            <!-- Event info + speaker avatar together -->
            <div class="flex-1 min-w-0">
              <p class="text-xs font-mono uppercase tracking-widest text-gray-400 mb-2">
                {{ event.category === 'basics' ? 'Basics' : 'Advanced' }} · {{ formatTime(event.scheduledAt) }} UTC
              </p>
              <h4 class="text-xl font-black text-[#231f20] leading-tight mb-2">{{ event.title }}</h4>
              <p class="text-sm text-gray-500 leading-relaxed mb-4">{{ event.description }}</p>

              <!-- Speaker row inside info column -->
              <div v-if="event.speaker" class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-full overflow-hidden flex-shrink-0">
                  <img
                    v-if="event.speakerPhotoUrl"
                    :src="event.speakerPhotoUrl"
                    :alt="event.speaker"
                    class="w-full h-full object-cover"
                  />
                  <div v-else class="w-full h-full bg-[#231f20] flex items-center justify-center">
                    <span class="text-[#c2f33d] text-[10px] font-black select-none">
                      {{ event.speaker.split(' ').map((w: string) => w[0]).slice(0, 2).join('').toUpperCase() }}
                    </span>
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
    </div>
  </div>
</template>
