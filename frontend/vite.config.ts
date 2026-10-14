import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // Alla anrop till /api skickas vidare till .NET-backenden.
    // Då slipper frontenden veta vilken port API:t kör på.
    proxy: {
      '/api': 'http://localhost:5000',
    },
  },
})
