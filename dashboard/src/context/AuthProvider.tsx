import { useState } from 'react'
import type { ReactNode } from 'react'
import { AuthContext } from './AuthContext'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('token'))
  const [email, setEmail] = useState<string | null>(() => localStorage.getItem('userEmail'))
  const [rol, setRol]     = useState<string | null>(() => localStorage.getItem('userRol'))

  const login = (t: string, e: string, r: string) => {
    localStorage.setItem('token',     t)
    localStorage.setItem('userEmail', e)
    localStorage.setItem('userRol',   r)
    setToken(t)
    setEmail(e)
    setRol(r)
  }

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('userEmail')
    localStorage.removeItem('userRol')
    setToken(null)
    setEmail(null)
    setRol(null)
  }

  return (
    <AuthContext.Provider value={{ token, email, rol, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}
