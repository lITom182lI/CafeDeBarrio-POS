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
  if (m.toLowerCase().includes('yape') || m.toLowerCase().includes('plin')) return 'badge badge-purple'
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
        <td>
        <div className="payment-methods-wrap">
          <span className={metodoBadge(tx.metodoPago)}>{tx.metodoPago}</span>
          {tx.metodoPagoSecundario && (
            <span className={metodoBadge(tx.metodoPagoSecundario)}>{tx.metodoPagoSecundario}</span>
          )}
        </div>
      </td>
        <td><span className="text-muted">{tx.operadorNombre ?? '—'}</span></td>
        <td>
          <div className="status-badges-wrap">
            <span className={tx.anulada ? 'badge badge-danger' : 'badge badge-ok'}>
              {tx.anulada ? 'Anulada' : 'Completada'}
            </span>
            {tx.tipoDocumento && (
              <span className="badge badge-purple badge-boleta">
                🧾 Boleta
              </span>
            )}
          </div>
        </td>
      </tr>
      {open && detalle && (
        <tr className="expand-row">
          <td colSpan={8}>
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
                {detalle.metodoPagoSecundario && detalle.montoMetodoPrimario && (
                  <>
                    <div className="detail-block">
                      <div className="detail-label">Pago 1</div>
                      <div className="detail-value">{detalle.metodoPago} — S/ {detalle.montoMetodoPrimario.toFixed(2)}</div>
                    </div>
                    <div className="detail-block">
                      <div className="detail-label">Pago 2</div>
                      <div className="detail-value">{detalle.metodoPagoSecundario} — S/ {(detalle.total - detalle.montoMetodoPrimario).toFixed(2)}</div>
                    </div>
                  </>
                )}
                <div className="detail-block">
                  <div className="detail-label">Operador</div>
                  <div className="detail-value">{detalle.operadorNombre ?? '—'}</div>
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
                {detalle.razonSocial && (
                  <>
                    <div className="detail-block boleta-section-title">
                      <div className="detail-label boleta-section-label">🧾 Boleta Nominada</div>
                    </div>
                    {detalle.tipoDocumento && detalle.numeroDocumento && (
                      <div className="detail-block">
                        <div className="detail-label">{detalle.tipoDocumento}</div>
                        <div className="detail-value">{detalle.numeroDocumento}</div>
                      </div>
                    )}
                    <div className="detail-block">
                      <div className="detail-label">Razón Social</div>
                      <div className="detail-value">{detalle.razonSocial}</div>
                    </div>
                  </>
                )}
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
