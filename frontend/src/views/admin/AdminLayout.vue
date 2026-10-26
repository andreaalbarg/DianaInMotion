<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'
import { useAdminAuthStore } from '../../stores/adminAuth'

const auth = useAdminAuthStore()
const router = useRouter()
const route = useRoute()

function logout() {
  auth.logout()
  router.push('/admin/login')
}

const navItems = [
  { to: '/admin/events', label: 'Events' },
  { to: '/admin/registrations', label: 'Registrations' },
  { to: '/admin/subscribers', label: 'Subscribers' },
  { to: '/admin/youtube', label: 'YouTube' },
]
</script>

<template>
  <div class="min-h-screen bg-[#231f20] text-white">
    <aside class="fixed top-0 left-0 h-full w-56 bg-black/40 border-r border-white/10 flex flex-col">
      <div class="p-6 border-b border-white/10">
        <div class="text-lg font-black tracking-tight">DIANA</div>
        <div class="text-xs text-white/40 font-semibold tracking-widest">IN MOTION</div>
      </div>
      <nav class="flex-1 p-4 space-y-1">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors"
          :class="route.path === item.to
            ? 'bg-[#c2f33d] text-[#231f20]'
            : 'text-white/60 hover:text-white hover:bg-white/5'"
        >
          {{ item.label }}
        </RouterLink>
      </nav>
      <div class="p-4 border-t border-white/10">
        <button
          @click="logout"
          class="w-full text-left px-4 py-2.5 text-sm text-white/40 hover:text-white rounded-lg hover:bg-white/5 transition-colors"
        >
          Sign out
        </button>
      </div>
    </aside>
    <main class="ml-56 p-8">
      <RouterView />
    </main>
  </div>
</template>
