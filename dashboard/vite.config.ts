import { defineConfig } from 'vitest/config'
import { loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiTarget = env.VITE_API_TARGET || process.env.VITE_API_TARGET || 'http://127.0.0.1:5138'
  return {
    plugins: [react()],
    build: {
      chunkSizeWarningLimit: 700,
    },
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: './src/test/setup.ts',
      // @ts-expect-error Vite test environment define
      define: {
        'import.meta.env.VITE_SENTRY_DSN': JSON.stringify(''),
      },
    },
    server: {
      port: 5173,
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true,
        },
      },
    },
  }
})
