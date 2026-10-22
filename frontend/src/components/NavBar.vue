<script setup lang="ts">
function scrollTo(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}
</script>

<template>
  <header class="fixed top-0 left-0 right-0 z-50 bg-white border-b border-gray-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 h-14 flex items-center justify-between">
      <div class="flex items-center gap-3">
        <div class="w-7 h-7 bg-[#c2f33d] flex items-center justify-center flex-shrink-0">
          <svg width="15" height="15" viewBox="0 0 18 18" fill="none">
            <rect x="2" y="3" width="2.5" height="12" rx="0" fill="#231f20"/>
            <rect x="5.5" y="3" width="2.5" height="12" rx="0" fill="#231f20"/>
            <path d="M10 5.5L15.5 9L10 12.5V5.5Z" fill="#231f20"/>
          </svg>
        </div>
        <div class="leading-none">
          <span class="text-[#231f20] font-black text-sm tracking-tight">Diana in Motion</span>
          <span class="block text-gray-400 font-mono text-[10px] uppercase tracking-widest">Live Training Series</span>
        </div>
      </div>

      <nav class="hidden md:flex items-center gap-8 text-sm text-gray-500">
        <button @click="scrollTo('upcoming')" class="hover:text-[#231f20] transition-colors">Upcoming</button>
        <button @click="scrollTo('ondemand')" class="hover:text-[#231f20] transition-colors">On Demand</button>
      </nav>

      <button
        @click="scrollTo('upcoming')"
        class="bg-[#231f20] text-white text-xs font-semibold px-5 py-2 hover:bg-[#3d3839] transition-colors"
      >
        Register
      </button>
    </div>
  </header>
</template>
