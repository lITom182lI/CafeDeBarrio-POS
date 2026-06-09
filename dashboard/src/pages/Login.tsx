import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export function Login() {
  const { login } = useAuth()
  const navigate  = useNavigate()
  const [email,    setEmail]    = useState('')
  const [password, setPassword] = useState('')
  const [error,    setError]    = useState('')
  const [loading,  setLoading]  = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')
    try {
      const r = await fetch('/api/auth/login', {
        method:  'POST',
        headers: { 'Content-Type': 'application/json' },
        body:    JSON.stringify({ email, password }),
      })
      if (!r.ok) {
        setError('Credenciales incorrectas')
        return
      }
      const data = await r.json() as { token: string; email: string; rol: string }
      login(data.token, data.email, data.rol)
      navigate('/', { replace: true })
    } catch {
      setError('No se pudo conectar con el servidor')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{
      minHeight: '100vh', display: 'flex', alignItems: 'center',
      justifyContent: 'center', background: 'var(--color-bg)',
    }}>
      <div style={{
        background: 'var(--color-card)', borderRadius: 'var(--radius)',
        border: '1px solid var(--color-border)', padding: '48px 40px',
        width: '100%', maxWidth: 400,
        boxShadow: '0 4px 24px rgba(0,0,0,0.06)',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 32 }}>
          <div style={{
            background: 'var(--color-brand)', borderRadius: 8,
            width: 36, height: 36, display: 'flex',
            alignItems: 'center', justifyContent: 'center',
            color: '#fff', fontSize: '1.1rem',
          }}>☕</div>
          <div>
            <div style={{ fontWeight: 800, fontSize: '1.1rem', color: 'var(--color-brand)', letterSpacing: '-0.03em' }}>
              Café de Barrio
            </div>
            <div style={{ fontSize: '0.65rem', color: 'var(--color-brand)', textTransform: 'uppercase', fontWeight: 700, letterSpacing: '0.08em' }}>
              Punto de Venta POS
            </div>
          </div>
        </div>

        <h2 style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--color-text-title)', marginBottom: 24 }}>
          Iniciar sesión
        </h2>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            <label style={{ fontSize: '0.82rem', fontWeight: 600, color: 'var(--color-text)' }}>
              Correo electrónico
            </label>
            <input
              type="email" value={email} onChange={e => setEmail(e.target.value)}
              required autoFocus
              placeholder="admin@cafedebarrio.com"
              style={{
                padding: '10px 14px', borderRadius: 10,
                border: '1.5px solid var(--color-border)',
                fontSize: '0.9rem', color: 'var(--color-text)',
                background: 'var(--color-bg)', outline: 'none',
                fontFamily: 'var(--font-sans)',
              }}
            />
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            <label style={{ fontSize: '0.82rem', fontWeight: 600, color: 'var(--color-text)' }}>
              Contraseña
            </label>
            <input
              type="password" value={password} onChange={e => setPassword(e.target.value)}
              required
              placeholder="••••••••"
              style={{
                padding: '10px 14px', borderRadius: 10,
                border: '1.5px solid var(--color-border)',
                fontSize: '0.9rem', color: 'var(--color-text)',
                background: 'var(--color-bg)', outline: 'none',
                fontFamily: 'var(--font-sans)',
              }}
            />
          </div>

          {error && (
            <div style={{
              padding: '10px 14px', borderRadius: 10,
              background: '#fef2f2', border: '1px solid #fecaca',
              color: 'var(--color-danger)', fontSize: '0.85rem',
            }}>
              {error}
            </div>
          )}

          <button
            type="submit" disabled={loading}
            style={{
              marginTop: 8, padding: '12px',
              background: loading ? 'var(--color-muted)' : 'var(--color-brand)',
              color: '#fff', border: 'none', borderRadius: 10,
              fontWeight: 700, fontSize: '0.95rem', cursor: loading ? 'not-allowed' : 'pointer',
              fontFamily: 'var(--font-sans)', transition: 'background 0.15s',
            }}
          >
            {loading ? 'Entrando…' : 'Ingresar'}
          </button>
        </form>
      </div>
    </div>
  )
}
