import { useState, useEffect } from 'react'
import type { OperadorSession } from './types'
import LoginScreen from './components/LoginScreen'
import SalesModule from './components/SalesModule'
import { setOperadorToken } from './api'
import { loadRemoteConfig } from './config'

export default function App() {
  const [session, setSession] = useState<OperadorSession | null>(null)

  // Auto resume session logic for high-end local POS systems
  useEffect(() => {
    void loadRemoteConfig()
    try {
      const storedToken = localStorage.getItem('CDB_TOKEN')
      const storedNombre = localStorage.getItem('CDB_SESSION_NAME')
      const storedId = localStorage.getItem('CDB_SESSION_ID')

      if (storedToken && storedNombre && storedId) {
        setOperadorToken(storedToken)
        setSession({
          operadorId: parseInt(storedId),
          nombre: storedNombre,
          token: storedToken
        })
      }
    } catch (err) {
      console.warn('Unable to restore previous Cajero session:', err)
    }
  }, [])

  const handleLogin = (newSession: OperadorSession | null) => {
    setSession(newSession)
    if (newSession) {
      // Persist to prevent terminal timeouts on page reloading
      localStorage.setItem('CDB_TOKEN', newSession.token || 'offline_generic_token')
      localStorage.setItem('CDB_SESSION_NAME', newSession.nombre)
      localStorage.setItem('CDB_SESSION_ID', String(newSession.operadorId))
    } else {
      localStorage.removeItem('CDB_TOKEN')
      localStorage.removeItem('CDB_SESSION_NAME')
      localStorage.removeItem('CDB_SESSION_ID')
    }
  }

  const handleLogout = () => {
    setSession(null)
    setOperadorToken(null)
    localStorage.removeItem('CDB_TOKEN')
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
