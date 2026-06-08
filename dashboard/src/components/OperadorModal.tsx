import { useState } from 'react'
import { api } from '../api/client'
import type { OperadorDto } from '../types'

interface Props {
  operador?: OperadorDto | null
  onClose: () => void
  onSaved: () => void
}

export function OperadorModal({ operador, onClose, onSaved }: Props) {
  const [nombre,   setNombre]   = useState(operador?.nombre ?? '')
  const [activo,   setActivo]   = useState(operador?.activo ?? true)
  const [pin,      setPin]      = useState('')
  const [saving,   setSaving]   = useState(false)
  const [err,      setErr]      = useState('')

  const validatePin = (p: string) => {
    if (!p) return true
    return /^\d{4,8}$/.test(p)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErr('')
    if (!nombre.trim()) { setErr('El nombre es obligatorio.'); return }
    if (!operador && !pin) { setErr('El PIN es obligatorio para nuevos operadores.'); return }
    if (pin && !validatePin(pin)) {
      setErr('El PIN debe tener entre 4 y 8 dígitos numéricos.'); return
    }
    setSaving(true)
    try {
      if (operador) {
        await api.actualizarOperador(operador.operadorId, {
          nombre,
          activo,
          nuevoPin: pin || undefined
        })
      } else {
        await api.crearOperador(nombre, pin)
      }
      onSaved()
    } catch {
      setErr('Error al guardar. Verifica los datos e intenta de nuevo.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="operador-modal-title"
        className="modal-box"
        onClick={e => e.stopPropagation()}
      >
        <div className="modal-header">
          <h2 id="operador-modal-title">{operador ? 'Editar Operador' : 'Nuevo Operador'}</h2>
          <button className="modal-close" aria-label="Cerrar" onClick={onClose}>✕</button>
        </div>
        <form onSubmit={handleSubmit} className="modal-form">
          {err && <div className="error-banner" role="alert">{err}</div>}

          <label htmlFor="operador-nombre">Nombre *</label>
          <input id="operador-nombre" value={nombre} onChange={e => setNombre(e.target.value)} required />

          <label htmlFor="operador-pin">
            {operador ? 'Nuevo PIN (dejar vacío para no cambiar)' : 'PIN * (4–8 dígitos)'}
          </label>
          <input
            id="operador-pin"
            type="password"
            inputMode="numeric"
            maxLength={8}
            value={pin}
            onChange={e => setPin(e.target.value.replace(/\D/g, ''))}
            placeholder={operador ? '••••' : ''}
            required={!operador}
          />

          {operador && (
            <label htmlFor="operador-activo" className="checkbox-label">
              <input id="operador-activo" type="checkbox" checked={activo}
                onChange={e => setActivo(e.target.checked)} />
              Activo
            </label>
          )}

          <div className="modal-actions">
            <button type="button" className="btn-secondary" onClick={onClose}>Cancelar</button>
            <button type="submit" className="btn-primary" disabled={saving}>
              {saving ? 'Guardando...' : (operador ? 'Guardar cambios' : 'Crear operador')}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
