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
            class="py-5 flex flex-col sm:flex-row sm:items-center justify-between gap-4"
          >
            <div class="flex gap-5 items-start">
              <!-- Date block -->
              <div class="flex-shrink-0 text-center w-12">
                <p class="text-3xl font-black text-[#231f20] leading-none">
                  {{ new Date(event.scheduledAt).getUTCDate() }}
                </p>
                <p class="text-xs font-mono text-gray-400 uppercase tracking-wide">
                  {{ new Date(event.scheduledAt).toLocaleDateString('en-US', { weekday: 'short', timeZone: 'UTC' }) }}
                </p>
              </div>

              <!-- Event info -->
              <div>
                <p class="text-xs font-mono uppercase tracking-widest text-gray-400 mb-1">
                  {{ event.category === 'basics' ? 'Basics' : 'Advanced' }} · {{ formatTime(event.scheduledAt) }} UTC
                </p>
                <h4 class="text-base font-black text-[#231f20] leading-tight">{{ event.title }}</h4>
                <p v-if="event.speaker" class="text-xs text-gray-400 font-mono mt-1">{{ event.speaker }}</p>
              </div>
            </div>

            <button
              @click="onRegister(event)"
              class="flex-shrink-0 bg-[#231f20] text-white text-xs font-semibold px-5 py-2 hover:bg-[#3d3839] transition-colors self-start sm:self-center"
            >
              Register →
            </button>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>
