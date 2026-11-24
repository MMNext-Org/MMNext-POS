import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import { VitePWA } from 'vite-plugin-pwa'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      injectRegister: 'script',
      includeAssets: ['favicon.png', 'apple-touch-icon.png', 'apple-touch-icon-precomposed.png', 'apple-touch-icon-precomposed-192x192.png', 'apple-touch-icon-precomposed-512x512.png'],
      manifest: {
        name: 'BREWFLOW POS',
        short_name: 'BREWFLOW',
        description: 'Coffee Shop POS System',
        start_url: '.',
        display: 'standalone',
        background_color: '#ffffff',
        theme_color: '#33691e',
        icons: [
          {
            src: 'favicon.png',
            sizes: '192x192',
            type: 'image/png'
          },
          {
            src: 'favicon.png',
            sizes: '512x512',
            type: 'image/png'
          }
        ]
      }
    })
  ],
  resolve: {
    alias: {
      '@/context': path.resolve(__dirname, 'src/context'),
      '@/utils': path.resolve(__dirname, 'src/utils'),
      '@/pages': path.resolve(__dirname, 'src/pages'),
      '@/components': path.resolve(__dirname, 'src/components'),
      '@/types': path.resolve(__dirname, 'src/types')
    }
  }
})
