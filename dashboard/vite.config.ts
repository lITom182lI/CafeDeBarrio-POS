import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  define: {
    'import.meta.env.MODE': JSON.stringify('test'),
    'import.meta.env.VITE_SENTRY_DSN': JSON.stringify(''),
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test/setup.ts',
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5138',
        changeOrigin: true
      }
    }
  }
})
