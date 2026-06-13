import { useState, useEffect } from 'react'
import type { OperadorSession } from './types'
import LoginScreen from './components/LoginScreen'
import SalesModule from './components/SalesModule'
import { setOperadorToken } from './api'
import { loadRemoteConfig } from './config'

export default function App() {
  const [session, setSession] = useState<OperadorSession | null>(() => {
    try {
      const storedToken = localStorage.getItem('pos_auth_token')
      const storedNombre = localStorage.getItem('CDB_SESSION_NAME')
      const storedId = localStorage.getItem('CDB_SESSION_ID')

      if (storedToken && storedNombre && storedId) {
        setOperadorToken(storedToken)
        return {
          operadorId: parseInt(storedId),
          nombre: storedNombre,
          token: storedToken
        }
      }
    } catch (err) {
      console.warn('Unable to restore previous Cajero session:', err)
    }
    return null;
  })

  // Sincroniza el token al adapter en cada cambio de sesión (cubre HMR y restauración)
  useEffect(() => {
    setOperadorToken(session?.token ?? null)
  }, [session])

  useEffect(() => {
    void loadRemoteConfig()
  }, [])

  const handleLogin = (newSession: OperadorSession | null) => {
    setSession(newSession)
    if (newSession) {
      // Persist to prevent terminal timeouts on page reloading
      if (newSession.token) {
        localStorage.setItem('pos_auth_token', newSession.token)
      }
      localStorage.setItem('CDB_SESSION_NAME', newSession.nombre)
      localStorage.setItem('CDB_SESSION_ID', String(newSession.operadorId))
    } else {
      localStorage.removeItem('pos_auth_token')
      localStorage.removeItem('CDB_SESSION_NAME')
      localStorage.removeItem('CDB_SESSION_ID')
    }
  }

  const handleLogout = () => {
    setSession(null)
    setOperadorToken(null)
    localStorage.removeItem('pos_auth_token')
    localStorage.removeItem('CDB_SESSION_NAME')
    localStorage.removeItem('CDB_SESSION_ID')
  }

  return (
    <div className="min-h-screen bg-[#F8FAFC] selection:bg-[#7C2D12]/10 selection:text-[#7C2D12] text-[#334155]">
      {session ? (
        <SalesModule session={session} onLogout={handleLogout} />
      ) : (
        <LoginScreen onLogin={handleLogin} />
      )}
    </div>
  )
}
