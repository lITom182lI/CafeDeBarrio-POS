import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg}'],
        runtimeCaching: [
          {
            urlPattern: /^https:\/\/.*\/api\/productos/,
            handler: 'StaleWhileRevalidate',
            options: { cacheName: 'productos-cache' }
          },
          {
            urlPattern: /^https:\/\/.*\/api\/metodos-pago/,
            handler: 'StaleWhileRevalidate',
            options: { cacheName: 'metodos-cache' }
          }
        ]
      },
      manifest: {
        name: 'Café de Barrio — POS',
        short_name: 'CDB POS',
        description: 'Sistema de punto de venta',
        theme_color: '#c2622a',
        background_color: '#ffffff',
        display: 'standalone',
        orientation: 'landscape',
        start_url: '/',
        icons: [
          { src: 'icon-192.png', sizes: '192x192', type: 'image/png' },
          { src: 'icon-512.png', sizes: '512x512', type: 'image/png' }
        ]
      },
      devOptions: {
        enabled: false
      }
    })
  ],
  define: {
    'import.meta.env.VITE_API_URL': JSON.stringify(''),
    'import.meta.env.VITE_SEDE_ID': JSON.stringify('1'),
    'import.meta.env.VITE_TASA_IGV': JSON.stringify('0.105'),
    'import.meta.env.VITE_SENTRY_DSN': JSON.stringify(''),
    'import.meta.env.MODE': JSON.stringify('test'),
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test/setup.ts',
  },
  server: {
    port: 5174,
    proxy: {
      '/api': {
        target: 'http://localhost:5138',
        changeOrigin: true
      }
    }
  }
})
