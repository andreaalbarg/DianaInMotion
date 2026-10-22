import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig({
  plugins: [vue()],
  server: {
    port: parseInt(process.env.PORT || '3000'),
    host: true,
    allowedHosts: true,
    proxy: {
      '/api': process.env.BACKEND_URL || 'http://localhost:5050'
    }
  }
});