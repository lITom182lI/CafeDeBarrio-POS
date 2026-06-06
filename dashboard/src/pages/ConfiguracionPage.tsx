import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { api } from '../api/client'

export function ConfiguracionPage() {
  const { logout } = useAuth()
  const navigate   = useNavigate()

  const [form, setForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  })
  const [saving, setSaving] = useState(false)
  const [err, setErr]       = useState('')
  const [ok, setOk]         = useState(false)

  const set = (k: keyof typeof form, v: string) =>
    setForm(f => ({ ...f, [k]: v }))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErr('')
    if (form.newPassword.length < 8) {
      setErr('La nueva contraseña debe tener al menos 8 caracteres.')
      return
    }
    if (form.newPassword !== form.confirmPassword) {
      setErr('Las contraseñas nuevas no coinciden.')
      return
    }
    if (form.currentPassword === form.newPassword) {
      setErr('La nueva contraseña debe ser diferente a la actual.')
      return
    }
    setSaving(true)
    try {
      await api.cambiarContrasena(form.currentPassword, form.newPassword)
      setOk(true)
      setTimeout(() => {
        logout()
        navigate('/login', { replace: true })
      }, 2000)
    } catch {
      setErr('Contraseña actual incorrecta o error de servidor.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <h1 className="page-title">Configuracion</h1>
      </div>

      <div className="settings-card">
        <h2 className="settings-section-title">Cambiar contraseña</h2>
        <p className="settings-hint">
          Despues de cambiar la contraseña seras redirigido al login.
        </p>

        {ok && (
          <div className="success-banner">
            Contraseña actualizada. Redirigiendo al login...
          </div>
        )}
        {err && <div className="error-banner">{err}</div>}

        <form onSubmit={handleSubmit} className="settings-form">
          <label>Contraseña actual
            <input
              type="password"
              value={form.currentPassword}
              onChange={e => set('currentPassword', e.target.value)}
              required
              autoComplete="current-password"
            />
          </label>
          <label>Nueva contraseña
            <input
              type="password"
              value={form.newPassword}
              onChange={e => set('newPassword', e.target.value)}
              required
              autoComplete="new-password"
              minLength={8}
            />
          </label>
          <label>Confirmar nueva contraseña
            <input
              type="password"
              value={form.confirmPassword}
              onChange={e => set('confirmPassword', e.target.value)}
              required
              autoComplete="new-password"
            />
          </label>
          <button type="submit" className="btn-primary" disabled={saving || ok}>
            {saving ? 'Guardando...' : 'Cambiar contraseña'}
          </button>
        </form>
      </div>
    </div>
  )
}
