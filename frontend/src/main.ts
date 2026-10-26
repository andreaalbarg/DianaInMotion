import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import Root from './Root.vue'
import router from './router'
import { setBackendUrl } from './services/config'

// Fetch the backend's public URL written by the backend at startup.
// This lets the browser call the backend directly (bypassing the Vite proxy).
try {
  const cfg = await fetch('/backend-config.json').then(r => r.json())
  if (cfg?.backendUrl) setBackendUrl(cfg.backendUrl)
} catch { /* local dev without the file — falls back to relative /api */ }

createApp(Root).use(createPinia()).use(router).mount('#app')
