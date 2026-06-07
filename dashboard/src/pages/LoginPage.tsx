import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export function LoginPage() {
  const [email, setEmail]       = useState('')
  const [password, setPassword] = useState('')
  const [error, setError]       = useState('')
  const [loading, setLoading]   = useState(false)
  const { login } = useAuth()
  const navigate  = useNavigate()

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true); setError('')
    try {
      const r = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      })
      if (!r.ok) { setError('Credenciales incorrectas'); return }
      const data = await r.json()
      login(data.token)
      navigate('/dashboard', { replace: true })
    } catch {
      setError('No se pudo conectar con el servidor')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="login-bg">
      <div className="login-card">
        <div className="login-brand">☕ Café de Barrio</div>
        <div className="login-subtitle">Panel de Administración</div>
        <form onSubmit={handleSubmit} className="login-form">
          <label className="login-label">Correo electrónico</label>
          <input className="login-input" type="email"
            placeholder="admin@cafedebarrio.com"
            value={email} onChange={e => setEmail(e.target.value)}
            required autoFocus />
          <label className="login-label">Contraseña</label>
          <input className="login-input" type="password"
            placeholder="••••••••"
            value={password} onChange={e => setPassword(e.target.value)}
            required />
          {error && <div className="login-error">{error}</div>}
          <button className="login-btn" type="submit" disabled={loading}>
            {loading ? 'Ingresando...' : 'Ingresar'}
          </button>
        </form>
      </div>
    </div>
  )
}
