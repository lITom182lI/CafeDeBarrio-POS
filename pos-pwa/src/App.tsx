import { useState } from 'react'
import type { OperadorSession } from './types'
import LoginScreen from './components/LoginScreen'
import SalesModule from './components/SalesModule'
import './index.css'

export default function App() {
  const [session, setSession] = useState<OperadorSession | null | undefined>(undefined)

  if (session === undefined) {
    return <LoginScreen onLogin={setSession} />
  }
  return <SalesModule session={session} onLogout={() => setSession(undefined)} />
}
