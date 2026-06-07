import { useState, useEffect, useMemo } from 'react'
import { TransaccionRow } from '../components/TransaccionRow'
import { api } from '../api/client'
import type { TransaccionListItemDto } from '../types'

export function TransaccionesPage() {
  const [txs, setTxs]         = useState<TransaccionListItemDto[]>([])
  const [busqueda, setBusqueda] = useState('')
  const [err, setErr]         = useState('')

  useEffect(() => {
    api.transacciones()
      .then(setTxs)
      .catch(() => setErr('No se pudo cargar las transacciones'))
  }, [])

  const filtradas = useMemo(() => {
    const q = busqueda.toLowerCase()
    return txs.filter(t =>
      String(t.transaccionId).includes(q) ||
      t.clienteNombre.toLowerCase().includes(q) ||
      String(t.total).includes(q) ||
      (t.operadorNombre ?? '').toLowerCase().includes(q)
    )
  }, [txs, busqueda])

  return (
    <div className="page">
      <div className="page-header">
        <h1 className="page-title">Transacciones</h1>
        <div className="page-meta">Total: {filtradas.length} transacciones</div>
      </div>

      {err && <div className="error-banner">{err}</div>}

      <div className="search-wrap">
        <span className="search-icon">🔍</span>
        <input placeholder="Buscar por ID, cliente o monto..."
          value={busqueda} onChange={e => setBusqueda(e.target.value)} />
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th className="w-40"></th>
              <th className="w-90">ID</th>
              <th>Cliente</th>
              <th className="w-100">Monto</th>
              <th className="w-80">Hora</th>
              <th className="w-110">Método</th>
              <th className="w-110">Operador</th>
              <th className="w-110">Estado</th>
            </tr>
          </thead>
          <tbody>
            {filtradas.length === 0
              ? <tr><td colSpan={8}><div className="empty-state">Sin transacciones</div></td></tr>
              : filtradas.map(tx => <TransaccionRow key={tx.transaccionId} tx={tx} />)
            }
          </tbody>
        </table>
      </div>
    </div>
  )
}
