import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig({
  plugins: [vue()],
  define: {
    // Injected by Diana at spawn: browser-reachable backend URL (e.g. https://pv1234.app.sourcecoding.ai)
    // Empty string in local dev → API clients fall back to relative /api proxy
    __BACKEND_URL__: JSON.stringify(process.env.BACKEND_PUBLIC_URL ?? ''),
  },
  server: {
    port: parseInt(process.env.PORT || '3000'),
    host: true,
    allowedHosts: true,
    proxy: {
      '/api': {
        target: process.env.BACKEND_URL || 'http://localhost:5050',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});