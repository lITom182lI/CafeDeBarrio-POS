import { useState } from 'react'

interface Props {
  title: string
  message: string
  onConfirm: () => void
  onCancel: () => void
}

export function ConfirmModal({ title, message, onConfirm, onCancel }: Props) {
  const [loading, setLoading] = useState(false)

  const handleConfirm = async () => {
    setLoading(true)
    await onConfirm()
    setLoading(false)
  }

  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div
        role="dialog"
        aria-modal="true"
        className="modal-box"
        onClick={e => e.stopPropagation()}
      >
        <div className="modal-header">
          <h2>{title}</h2>
          <button className="modal-close" aria-label="Cerrar" onClick={onCancel}>✕</button>
        </div>
        <div className="modal-body" style={{ padding: '1rem' }}>
          <p>{message}</p>
        </div>
        <div className="modal-actions">
          <button type="button" className="btn-secondary" onClick={onCancel} disabled={loading}>
            Cancelar
          </button>
          <button type="button" className="btn-primary" style={{ backgroundColor: 'red', borderColor: 'red' }} onClick={handleConfirm} disabled={loading}>
            {loading ? 'Eliminando...' : 'Eliminar'}
          </button>
        </div>
      </div>
    </div>
  )
}
