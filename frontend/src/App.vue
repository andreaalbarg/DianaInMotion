<script setup lang="ts">
import { ref, onMounted } from 'vue'
import NavBar from './components/NavBar.vue'
import EventCard from './components/EventCard.vue'
import RegistrationModal from './components/RegistrationModal.vue'
import { fetchUpcomingEvents, fetchPastEvents } from './services/api'
import type { Event } from './types'

const upcomingEvents = ref<Event[]>([])
const pastEvents = ref<Event[]>([])
const selectedEvent = ref<Event | null>(null)
const loadingUpcoming = ref(true)
const loadingPast = ref(true)

onMounted(async () => {
  try {
    upcomingEvents.value = await fetchUpcomingEvents()
  } finally {
    loadingUpcoming.value = false
  }
  try {
    pastEvents.value = await fetchPastEvents()
  } finally {
    loadingPast.value = false
  }
})

function openModal(event: Event) {
  selectedEvent.value = event
}

function onRegistered() {
  if (selectedEvent.value) {
    const ev = upcomingEvents.value.find(e => e.id === selectedEvent.value!.id)
    if (ev) ev.registrationCount++
  }
}

function scrollToSection(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}
</script>

<template>
  <NavBar />

  <!-- Hero -->
  <section class="pt-16 min-h-[92vh] flex flex-col items-center justify-center text-center px-6 relative overflow-hidden">
    <!-- background glow -->
    <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] rounded-full bg-[#A8FF00]/5 blur-[120px] pointer-events-none" />

    <div class="relative max-w-3xl mx-auto">
      <div class="inline-flex items-center gap-2 bg-[#151719] border border-[#2A2D35] rounded-full px-4 py-1.5 text-sm text-[#9CA3AF] mb-8">
        <span class="w-2 h-2 rounded-full bg-[#A8FF00] animate-pulse"></span>
        Sesiones en vivo cada semana
      </div>

      <h1 class="text-white font-bold text-5xl md:text-6xl leading-tight mb-6 tracking-tight">
        Aprende a construir con<br/>
        <span class="text-[#A8FF00]">Diana</span>, en vivo
      </h1>

      <p class="text-[#9CA3AF] text-lg md:text-xl max-w-xl mx-auto leading-relaxed mb-10">
        Sesiones semanales donde la comunidad descubre funcionalidades, aprende flujos de trabajo
        y observa cómo transformar ideas en software real.
      </p>

      <div class="flex flex-wrap items-center justify-center gap-4">
        <button
          @click="scrollToSection('upcoming')"
          class="bg-[#A8FF00] text-[#0B0C0E] font-bold px-8 py-3.5 rounded-xl text-base hover:bg-[#BFFF33] transition-colors"
        >
          Ver próximas sesiones
        </button>
        <button
          @click="scrollToSection('ondemand')"
          class="bg-[#151719] border border-[#2A2D35] text-white font-semibold px-8 py-3.5 rounded-xl text-base hover:border-[#A8FF00] hover:text-[#A8FF00] transition-colors"
        >
          Ver grabaciones
        </button>
      </div>

      <!-- Categories -->
      <div class="mt-16 grid grid-cols-1 sm:grid-cols-2 gap-4 text-left max-w-2xl mx-auto">
        <div class="bg-[#151719] border border-[#2A2D35] rounded-xl p-5">
          <div class="w-9 h-9 rounded-lg bg-emerald-900/50 flex items-center justify-center mb-3">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#34d399" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 2L2 7l10 5 10-5-10-5z"/><path d="M2 17l10 5 10-5"/><path d="M2 12l10 5 10-5"/>
            </svg>
          </div>
          <h3 class="text-white font-semibold mb-1">In Motion / Basics</h3>
          <p class="text-[#6B7280] text-sm">Primeros pasos, funcionalidades y flujos esenciales de Diana.</p>
        </div>
        <div class="bg-[#151719] border border-[#2A2D35] rounded-xl p-5">
          <div class="w-9 h-9 rounded-lg bg-purple-900/50 flex items-center justify-center mb-3">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#c084fc" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/>
            </svg>
          </div>
          <h3 class="text-white font-semibold mb-1">In Motion / Advanced</h3>
          <p class="text-[#6B7280] text-sm">Proyectos avanzados, integraciones y flujos de trabajo complejos.</p>
        </div>
      </div>
    </div>
  </section>

  <!-- Upcoming Events -->
  <section id="upcoming" class="py-20 px-6">
    <div class="max-w-6xl mx-auto">
      <div class="mb-10">
        <h2 class="text-white font-bold text-3xl md:text-4xl mb-3">Próximas sesiones</h2>
        <p class="text-[#6B7280] text-base">Regístrate y recibe el link de acceso antes del evento.</p>
      </div>

      <div v-if="loadingUpcoming" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        <div v-for="i in 3" :key="i" class="bg-[#151719] border border-[#2A2D35] rounded-2xl p-5 h-52 animate-pulse" />
      </div>

      <div v-else-if="upcomingEvents.length === 0" class="text-center py-16 text-[#4B5563]">
        No hay sesiones programadas por el momento. Vuelve pronto.
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        <EventCard
          v-for="event in upcomingEvents"
          :key="event.id"
          :event="event"
          :on-register="() => openModal(event)"
        />
      </div>
    </div>
  </section>

  <!-- Divider -->
  <div class="max-w-6xl mx-auto px-6">
    <div class="border-t border-[#2A2D35]" />
  </div>

  <!-- On Demand -->
  <section id="ondemand" class="py-20 px-6">
    <div class="max-w-6xl mx-auto">
      <div class="mb-10">
        <div class="inline-flex items-center gap-2 bg-[#151719] border border-[#2A2D35] rounded-full px-3 py-1 text-xs text-[#9CA3AF] mb-4">
          <svg width="12" height="12" viewBox="0 0 24 24" fill="currentColor" class="text-[#A8FF00]"><path d="M8 5v14l11-7z"/></svg>
          On Demand
        </div>
        <h2 class="text-white font-bold text-3xl md:text-4xl mb-3">Sesiones anteriores</h2>
        <p class="text-[#6B7280] text-base">Aprende a tu propio ritmo con las grabaciones de sesiones pasadas.</p>
      </div>

      <div v-if="loadingPast" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        <div v-for="i in 3" :key="i" class="bg-[#151719] border border-[#2A2D35] rounded-2xl p-5 h-52 animate-pulse" />
      </div>

      <div v-else-if="pastEvents.length === 0" class="text-center py-16 text-[#4B5563]">
        Las grabaciones aparecerán aquí después de cada sesión.
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        <EventCard
          v-for="event in pastEvents"
          :key="event.id"
          :event="event"
        />
      </div>
    </div>
  </section>

  <!-- Footer -->
  <footer class="border-t border-[#1E2028] py-10 px-6 mt-auto">
    <div class="max-w-6xl mx-auto flex flex-col md:flex-row items-center justify-between gap-4">
      <div class="flex items-center gap-3">
        <div class="w-7 h-7 rounded-lg bg-[#A8FF00] flex items-center justify-center">
          <svg width="14" height="14" viewBox="0 0 18 18" fill="none">
            <rect x="2" y="3" width="2.5" height="12" rx="1" fill="#0B0C0E"/>
            <rect x="5.5" y="3" width="2.5" height="12" rx="1" fill="#0B0C0E"/>
            <path d="M10 5.5L15.5 9L10 12.5V5.5Z" fill="#0B0C0E"/>
          </svg>
        </div>
        <span class="text-[#6B7280] text-sm">Diana in Motion · Live Training Series</span>
      </div>
      <p class="text-[#4B5563] text-xs">Ideas into action · Sesiones semanales para la comunidad de Diana</p>
    </div>
  </footer>

  <!-- Registration Modal -->
  <RegistrationModal
    v-if="selectedEvent"
    :event="selectedEvent"
    @close="selectedEvent = null"
    @registered="onRegistered"
  />
</template>
