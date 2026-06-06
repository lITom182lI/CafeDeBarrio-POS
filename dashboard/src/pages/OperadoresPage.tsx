import { useState, useEffect } from 'react'
import { api } from '../api/client'
import type { OperadorDto } from '../types'
import { OperadorModal } from '../components/OperadorModal'

export function OperadoresPage() {
  const [operadores, setOperadores] = useState<OperadorDto[]>([])
  const [err,        setErr]        = useState('')
  const [modal,      setModal]      = useState<{ open: boolean; operador?: OperadorDto | null }>({ open: false })

  const cargar = () =>
    api.operadores().then(setOperadores).catch(() => setErr('No se pudo cargar la lista.'))

  useEffect(() => { cargar() }, [])

  return (
    <div className="page">
      <div className="page-header">
        <h1 className="page-title">Operadores</h1>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <div className="page-meta">Total: {operadores.length}</div>
          <button className="btn-primary"
            onClick={() => setModal({ open: true, operador: null })}>
            + Nuevo Operador
          </button>
        </div>
      </div>

      {err && <div className="error-banner">{err}</div>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr><th>Nombre</th><th>Estado</th><th></th></tr>
          </thead>
          <tbody>
            {operadores.length === 0
              ? <tr><td colSpan={3}><div className="empty-state">Sin operadores registrados</div></td></tr>
              : operadores.map(op => (
                <tr key={op.operadorId} className={`data-row${!op.activo ? ' row-inactive' : ''}`}>
                  <td className="fw-medium">{op.nombre}</td>
                  <td>
                    <span className={`badge ${op.activo ? 'badge-green' : 'badge-gray'}`}>
                      {op.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td>
                    <button className="btn-link"
                      onClick={() => setModal({ open: true, operador: op })}>
                      Editar
                    </button>
                  </td>
                </tr>
              ))
            }
          </tbody>
        </table>
      </div>

      {modal.open && (
        <OperadorModal
          operador={modal.operador}
          onClose={() => setModal({ open: false })}
          onSaved={() => { setModal({ open: false }); cargar() }}
        />
      )}
    </div>
  )
}
