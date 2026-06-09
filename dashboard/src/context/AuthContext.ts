import { createContext } from 'react'

export interface AuthCtx {
  token: string | null
  email: string | null
  rol: string | null
  login: (token: string, email: string, rol: string) => void
  logout: () => void
}

export const AuthContext = createContext<AuthCtx | null>(null)
