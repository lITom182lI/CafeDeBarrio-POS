import { StrictMode, type ReactNode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import './index.css'
import App from './App.tsx'
import { Login } from './pages/Login.tsx'
import { AuthProvider } from './context/AuthProvider.tsx'
import { useAuth } from './hooks/useAuth.ts'
import { initTelemetry, captureError } from './lib/telemetry'
import { AppErrorBoundary } from './components/AppErrorBoundary'

initTelemetry()

window.addEventListener('unhandledrejection', (e) => {
  captureError(e.reason, { type: 'unhandledrejection' })
})

function PrivateRoute({ children }: { children: ReactNode }) {
  const { token } = useAuth()
  return token ? <>{children}</> : <Navigate to="/login" replace />
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AppErrorBoundary>
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route path="/*" element={
              <PrivateRoute>
                <App />
              </PrivateRoute>
            } />
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </AppErrorBoundary>
  </StrictMode>
)
