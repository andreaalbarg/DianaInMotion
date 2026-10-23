<script setup lang="ts">
import { ref, onMounted } from 'vue'
import NavBar from './components/NavBar.vue'
import CalendarSchedule from './components/CalendarSchedule.vue'
import RegistrationModal from './components/RegistrationModal.vue'
import EventCard from './components/EventCard.vue'
import { fetchUpcomingEvents, fetchPastEvents, subscribeNewsletter } from './services/api'
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

const newsletterEmail = ref('')
const newsletterState = ref<'idle' | 'loading' | 'success' | 'error'>('idle')
const newsletterMsg = ref('')

async function submitNewsletter() {
  if (!newsletterEmail.value.trim()) return
  newsletterState.value = 'loading'
  try {
    const res = await subscribeNewsletter(newsletterEmail.value.trim())
    newsletterMsg.value = res.message
    newsletterState.value = 'success'
    newsletterEmail.value = ''
  } catch {
    newsletterMsg.value = 'Something went wrong. Please try again.'
    newsletterState.value = 'error'
  }
}
</script>

<template>
  <div class="min-h-screen bg-white text-[#231f20]">
    <NavBar />

    <!-- Hero -->
    <section class="pt-16">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 pt-16 pb-16 border-b border-gray-100">

        <div class="grid md:grid-cols-2 gap-0 items-center">
          <div class="pr-0 md:pr-16">
            <div class="flex items-center gap-2 mb-8">
              <span class="w-2 h-2 bg-[#c2f33d] block"></span>
              <span class="text-xs font-mono uppercase tracking-widest text-gray-400">Live every week</span>
            </div>

            <h1 class="text-6xl sm:text-7xl lg:text-8xl font-black text-[#231f20] leading-none tracking-tight mb-10 whitespace-pre-line">
              Learn to build<br/>with Diana,<br/><span class="text-live"><span class="live-ghost live-ghost-1" aria-hidden="true">LIVE.</span><span class="live-ghost live-ghost-2" aria-hidden="true">LIVE.</span><span class="live-solid">LIVE.</span></span>
            </h1>

            <p class="text-lg text-gray-500 leading-relaxed mb-10 max-w-md">
              Weekly live sessions where the community discovers features, learns workflows, and watches ideas turn into real software.
            </p>

            <div class="flex flex-wrap gap-3">
              <button
                @click="scrollToSection('upcoming')"
                class="px-6 py-3 bg-[#231f20] text-white text-sm font-semibold hover:bg-[#3d3839] transition-colors"
              >
                View upcoming sessions
              </button>
              <button
                @click="scrollToSection('ondemand')"
                class="px-6 py-3 border border-gray-300 text-gray-700 text-sm font-semibold hover:bg-gray-50 transition-colors"
              >
                Watch recordings
              </button>
            </div>
          </div>

          <!-- Categories card -->
          <div class="hidden md:flex flex-col gap-4">
            <div class="border border-gray-200 p-6">
              <p class="text-xs font-mono uppercase tracking-widest text-gray-400 mb-4">Track 01</p>
              <h3 class="text-xl font-black text-[#231f20] mb-2">In Motion / Basics</h3>
              <p class="text-sm text-gray-500 leading-relaxed">First steps, core features and essential workflows to start building with Diana.</p>
            </div>
            <div class="border border-gray-200 p-6">
              <p class="text-xs font-mono uppercase tracking-widest text-gray-400 mb-4">Track 02</p>
              <h3 class="text-xl font-black text-[#231f20] mb-2">In Motion / Advanced</h3>
              <p class="text-sm text-gray-500 leading-relaxed">Complex projects, integrations, external APIs, and advanced deployment workflows.</p>
            </div>
            <div class="border border-gray-200 p-6">
              <p class="text-xs font-mono uppercase tracking-widest text-gray-400 mb-4">Track 03</p>
              <h3 class="text-xl font-black text-[#231f20] mb-2">In Motion / On Demand</h3>
              <p class="text-sm text-gray-500 leading-relaxed">Recordings of past sessions — learn at your own pace, anytime.</p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Upcoming Sessions + Full Schedule -->
    <section id="upcoming" class="bg-gray-50 border-b border-gray-100">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 py-16">
        <div class="section-tag">
          <span class="tag-num">02</span>
          <span class="tag-label">Upcoming sessions</span>
          <span class="tag-rule" style="background:#e0e0e0"></span>
        </div>

        <div class="flex items-end justify-between gap-6 mb-10">
          <div>
            <h2 class="text-4xl sm:text-5xl font-black text-[#231f20] leading-tight mb-3">What's coming up</h2>
            <p class="text-gray-500 leading-relaxed max-w-md">Register to get the access link sent to your inbox before the session starts.</p>
          </div>
        </div>

        <div v-if="loadingUpcoming" class="space-y-6">
          <div v-for="i in 3" :key="i" class="border border-gray-200 h-36 animate-pulse bg-gray-100" />
        </div>

        <div v-else-if="upcomingEvents.length === 0" class="border border-gray-200 p-12 text-center">
          <p class="text-xs font-mono uppercase tracking-widest text-gray-400">No sessions scheduled right now — check back soon.</p>
        </div>

        <CalendarSchedule
          v-else
          :events="upcomingEvents"
          :on-register="openModal"
        />
      </div>
    </section>

    <!-- On Demand -->
    <section id="ondemand" class="border-b border-gray-100">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 py-16">
        <div class="section-tag">
          <span class="tag-num">03</span>
          <span class="tag-label">On Demand</span>
          <span class="tag-rule"></span>
        </div>

        <div class="flex items-end justify-between gap-6 mb-10">
          <div>
            <h2 class="text-4xl sm:text-5xl font-black text-[#231f20] leading-tight mb-3">Past sessions</h2>
            <p class="text-gray-500 leading-relaxed max-w-md">All previous sessions are recorded. Learn at your own pace — wherever you are.</p>
          </div>
          <a
            href="https://www.youtube.com/@dianainmotion"
            target="_blank"
            rel="noopener noreferrer"
            class="flex-shrink-0 flex items-center gap-2 border border-[#231f20] text-[#231f20] text-sm font-semibold px-6 py-3 hover:bg-[#231f20] hover:text-white transition-colors"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M8 5v14l11-7z"/></svg>
            Watch all
          </a>
        </div>

        <div v-if="loadingPast" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div v-for="i in 3" :key="i" class="border border-gray-200 p-6 h-56 animate-pulse bg-gray-100" />
        </div>

        <div v-else-if="pastEvents.length === 0" class="border border-gray-200 p-12 text-center">
          <p class="text-xs font-mono uppercase tracking-widest text-gray-400">Recordings will appear here after each session.</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <EventCard
            v-for="event in pastEvents"
            :key="event.id"
            :event="event"
          />
        </div>
      </div>
    </section>

    <!-- Bottom CTA + Newsletter -->
    <section class="bg-[#231f20] py-20">
      <div class="max-w-7xl mx-auto px-4 sm:px-6">
        <div class="section-tag">
          <span class="tag-num" style="background:#fff;color:#231f20">04</span>
          <span class="tag-label" style="color:#666">Get started</span>
          <span class="tag-rule" style="background:#333"></span>
        </div>

        <div class="grid md:grid-cols-2 gap-16 items-start">

          <!-- Left: CTA -->
          <div>
            <p class="text-xs font-mono uppercase tracking-widest text-gray-500 mb-6">Diana in Motion · Ideas into action</p>
            <h2 class="text-5xl sm:text-6xl font-black text-white leading-none tracking-tight mb-10">
              Turn your ideas<br/>into software.
            </h2>
            <div class="flex flex-wrap gap-3">
              <button
                @click="scrollToSection('upcoming')"
                class="px-6 py-3 bg-white text-[#231f20] text-sm font-semibold hover:bg-gray-100 transition-colors"
              >
                Register for a session
              </button>
              <a
                href="https://app.sourcecoding.ai"
                target="_blank"
                rel="noopener"
                class="px-6 py-3 text-gray-500 text-sm self-center hover:text-gray-300 transition-colors"
              >
                Try Diana for free →
              </a>
            </div>
          </div>

          <!-- Right: Newsletter -->
          <div class="border border-[#2d2829] p-8">
            <p class="text-xs font-mono uppercase tracking-widest text-gray-500 mb-4">Stay in the loop</p>
            <h3 class="text-2xl font-black text-white leading-tight mb-2">Never miss a session.</h3>
            <p class="text-gray-400 text-sm leading-relaxed mb-8">Get notified before each live session — no spam, just the essentials.</p>

            <template v-if="newsletterState !== 'success'">
              <div class="flex flex-col gap-3">
                <input
                  v-model="newsletterEmail"
                  type="email"
                  placeholder="your@email.com"
                  @keydown.enter="submitNewsletter"
                  class="w-full border border-[#3d3839] bg-transparent px-4 py-3 text-sm text-white placeholder-gray-600 focus:outline-none focus:border-[#c2f33d] transition-colors"
                />
                <button
                  @click="submitNewsletter"
                  :disabled="newsletterState === 'loading'"
                  class="w-full bg-[#c2f33d] text-[#231f20] text-sm font-semibold py-3 hover:bg-[#d4ff50] transition-colors disabled:opacity-50"
                >
                  {{ newsletterState === 'loading' ? 'Subscribing…' : 'Subscribe →' }}
                </button>
              </div>
              <p v-if="newsletterState === 'error'" class="mt-3 text-xs font-mono text-red-400">{{ newsletterMsg }}</p>
            </template>

            <div v-else class="flex items-center gap-3">
              <div class="w-8 h-8 bg-[#c2f33d] flex items-center justify-center flex-shrink-0">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#231f20" stroke-width="3" stroke-linecap="square"><polyline points="20 6 9 17 4 12"/></svg>
              </div>
              <p class="text-sm font-semibold text-white">{{ newsletterMsg }}</p>
            </div>
          </div>

        </div>
      </div>
    </section>

    <!-- Footer -->
    <footer class="bg-[#231f20] border-t border-[#2d2829] py-8">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div class="flex items-center gap-3">
          <img src="/logo-diana-in-motion.png" alt="Diana in Motion" class="h-5 w-auto brightness-0 invert opacity-50" />
        </div>
        <p class="text-gray-600 text-xs font-mono">sourcecoding.ai · Weekly community sessions</p>
      </div>
    </footer>

    <!-- Registration Modal -->
    <RegistrationModal
      v-if="selectedEvent"
      :event="selectedEvent"
      @close="selectedEvent = null"
      @registered="onRegistered"
    />
  </div>
</template>
