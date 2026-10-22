<script setup lang="ts">
function scrollTo(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}
</script>

<template>
  <header class="fixed top-0 left-0 right-0 z-50 bg-[#0B0C0E]/90 backdrop-blur-sm border-b border-[#1E2028]">
    <div class="max-w-6xl mx-auto px-6 h-16 flex items-center justify-between">
      <div class="flex items-center gap-3">
        <div class="w-8 h-8 rounded-lg bg-[#A8FF00] flex items-center justify-center">
          <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
            <rect x="2" y="3" width="2.5" height="12" rx="1" fill="#0B0C0E"/>
            <rect x="5.5" y="3" width="2.5" height="12" rx="1" fill="#0B0C0E"/>
            <path d="M10 5.5L15.5 9L10 12.5V5.5Z" fill="#0B0C0E"/>
          </svg>
        </div>
        <div>
          <span class="text-white font-semibold text-sm leading-none">Diana in Motion</span>
          <span class="block text-[#6B7280] text-xs">Live Training Series</span>
        </div>
      </div>

      <nav class="hidden md:flex items-center gap-6 text-sm text-[#9CA3AF]">
        <button @click="scrollTo('upcoming')" class="hover:text-white transition-colors">Próximas sesiones</button>
        <button @click="scrollTo('ondemand')" class="hover:text-white transition-colors">On Demand</button>
      </nav>

      <button
        @click="scrollTo('upcoming')"
        class="bg-[#A8FF00] text-[#0B0C0E] text-sm font-semibold px-4 py-2 rounded-lg hover:bg-[#BFFF33] transition-colors"
      >
        Registrarme
      </button>
    </div>
  </header>
</template>
