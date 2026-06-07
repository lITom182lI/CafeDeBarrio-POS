import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { initTelemetry } from './lib/telemetry'
import { AppErrorBoundary } from './components/AppErrorBoundary'

initTelemetry()

window.addEventListener('unhandledrejection', (e) => {
  import('./lib/telemetry').then(({ captureError }) =>
    captureError(e.reason, { type: 'unhandledrejection' })
  )
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AppErrorBoundary>
      <App />
    </AppErrorBoundary>
  </StrictMode>
)
