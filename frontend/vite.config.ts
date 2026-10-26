import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import fs from 'fs';
import os from 'os';
import path from 'path';

// Read backend URL from the discovery file the backend writes at startup.
// This is more reliable than BACKEND_URL / BACKEND_PUBLIC_URL which Diana
// may inject with a stale port from a different project's session.
function backendTarget(): string {
  try {
    const url = fs.readFileSync(path.join(os.tmpdir(), 'diana_dianainmotion_backend'), 'utf-8').trim();
    if (url) return url;
  } catch {}
  return process.env.BACKEND_URL || 'http://localhost:5050';
}

export default defineConfig({
  plugins: [vue()],
  server: {
    port: parseInt(process.env.PORT || '3000'),
    host: true,
    allowedHosts: true,
    proxy: {
      '/api': {
        target: backendTarget(),
        changeOrigin: true,
        secure: false,
      },
    },
  },
});