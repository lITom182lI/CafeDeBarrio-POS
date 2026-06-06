import { useState } from 'react'
import type { TransaccionListItemDto, TransaccionDetalleDto } from '../types'
import { api } from '../api/client'

interface Props { tx: TransaccionListItemDto }

const fmtS    = (n: number) => `S/ ${n.toFixed(2)}`
const fmtHora = (s: string) => new Date(s).toLocaleTimeString('es-PE', { hour:'2-digit', minute:'2-digit' })
const fmtId   = (id: number) => `TXN${String(id).padStart(3,'0')}`

function metodoBadge(m: string) {
  if (m.toLowerCase().includes('tarjeta')) return 'badge badge-blue'
  if (m.toLowerCase().includes('efectivo')) return 'badge badge-gray'
  if (m.toLowerCase().includes('qr') || m.toLowerCase().includes('digital')) return 'badge badge-purple'
  return 'badge badge-gray'
}

export function TransaccionRow({ tx }: Props) {
  const [open, setOpen]       = useState(false)
  const [detalle, setDetalle] = useState<TransaccionDetalleDto | null>(null)
  const [loading, setLoading] = useState(false)

  const toggle = async () => {
    if (!open && !detalle) {
      setLoading(true)
      try { setDetalle(await api.transaccionDetalle(tx.transaccionId)) }
      catch { /* silencioso */ }
      finally { setLoading(false) }
    }
    setOpen(o => !o)
  }

  return (
    <>
      <tr className="data-row">
        <td>
          <button className={`chevron-btn${open?' open':''}`} onClick={toggle}>
            {loading ? '…' : open ? '▲' : '▼'}
          </button>
        </td>
        <td className="fw-semibold">{fmtId(tx.transaccionId)}</td>
        <td>{tx.clienteNombre}</td>
        <td className="fw-semibold">{fmtS(tx.total)}</td>
        <td>{fmtHora(tx.fecha)}</td>
        <td><span className={metodoBadge(tx.metodoPago)}>{tx.metodoPago}</span></td>
        <td>
          <span className={tx.anulada ? 'badge badge-danger' : 'badge badge-ok'}>
            {tx.anulada ? 'Anulada' : 'Completada'}
          </span>
        </td>
      </tr>
      {open && detalle && (
        <tr className="expand-row">
          <td colSpan={7}>
            <div className="expand-inner">
              <div>
                <div className="detail-block">
                  <div className="detail-label">ID Transacción</div>
                  <div className="detail-value">{fmtId(detalle.transaccionId)}</div>
                </div>
                <div className="detail-block">
                  <div className="detail-label">Cliente</div>
                  <div className="detail-value">{detalle.clienteNombre}</div>
                </div>
                <div className="detail-block">
                  <div className="detail-label">Hora</div>
                  <div className="detail-value">{fmtHora(detalle.fecha)}</div>
                </div>
                <div className="detail-block">
                  <div className="detail-label">Método de Pago</div>
                  <div><span className={metodoBadge(detalle.metodoPago)}>{detalle.metodoPago}</span></div>
                </div>
                <div className="detail-block">
                  <div className="detail-label">Subtotal / IGV</div>
                  <div className="detail-value">{fmtS(detalle.subtotal)} / {fmtS(detalle.igv)}</div>
                </div>
              </div>
              <div>
                <div className="detail-label mb-8">Artículos</div>
                {detalle.items.map((item, i) => (
                  <div key={i} className="art-item">
                    <span className="art-dot" />
                    <span>{item.nombreProducto}</span>
                    <span className="ml-auto text-muted">x{item.cantidad}</span>
                  </div>
                ))}
                <div className="expand-total">
                  <span>Total</span>
                  <span className="expand-total-val">{fmtS(detalle.total)}</span>
                </div>
              </div>
            </div>
          </td>
        </tr>
      )}
    </>
  )
}
