<script setup lang="ts">
function scrollTo(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}
</script>

<template>
  <header class="fixed top-0 left-0 right-0 z-50 bg-white border-b border-gray-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 h-14 flex items-center justify-between">
      <div class="flex items-center gap-3">
        <img src="/logo-diana-in-motion.png" alt="Diana in Motion" class="h-9 w-auto" />
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
