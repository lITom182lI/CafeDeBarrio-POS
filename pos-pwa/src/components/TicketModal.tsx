import PrintReceipt from './PrintReceipt'
import type { TicketData } from '../types'
import { formatSoles } from '../utils'
import { config } from '../config'

// Re-exportar para que SalesModule.tsx no requiera cambios de import
export type { TicketData }

interface Props {
  ticket: TicketData
  onClose: () => void
}

export default function TicketModal({ ticket, onClose }: Props) {
  const fecha = new Date(ticket.fechaHora).toLocaleString('es-PE', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })

  return (
    <>
      {/* Visible sólo durante window.print() */}
      <PrintReceipt ticket={ticket} />

      {/* Modal visual — se oculta al imprimir */}
      <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4 print:hidden">
        <div className="bg-stone-800 rounded-xl shadow-2xl w-full max-w-sm p-6 space-y-4 max-h-[90vh] overflow-y-auto">

          {/* Estado */}
          <div className="text-center">
            <div className="text-4xl mb-2">{ticket.offline ? '🔄' : '✅'}</div>
            <h2
              className={`font-bold text-lg ${ticket.offline ? 'text-amber-500' : 'text-green-500'}`}
            >
              {ticket.offline ? 'Guardada sin conexión' : 'Venta registrada'}
            </h2>
            {ticket.offline
              ? <p className="text-stone-400 text-xs mt-1">Se sincronizará automáticamente al reconectarse</p>
              : <p className="text-stone-400 text-xs mt-1">Transacción #{ticket.transaccionId}</p>
            }
            <p className="text-stone-500 text-xs mt-1">{fecha}</p>
          </div>

          <div className="border-t border-stone-700" />

          {/* Boleta nominada */}
          {ticket.comprobante && (
            <div className="bg-amber-900/30 border border-amber-700 rounded-lg p-3 text-sm">
              <p className="text-amber-300 font-semibold text-xs uppercase tracking-wide mb-1">Boleta nominada</p>
              <p className="text-stone-200">{ticket.comprobante.tipoDocumento}: {ticket.comprobante.numeroDocumento}</p>
              <p className="text-stone-200">{ticket.comprobante.razonSocial}</p>
            </div>
          )}

          {/* Items */}
          <div className="space-y-1">
            {ticket.items.map(item => (
              <div key={item.productoId} className="flex justify-between text-sm">
                <span className="text-stone-300">
                  {item.nombre} <span className="text-stone-500">×{item.cantidad}</span>
                </span>
                <span className="text-stone-200">{formatSoles(item.precio * item.cantidad)}</span>
              </div>
            ))}
          </div>

          <div className="border-t border-stone-700" />

          {/* Totales */}
          <div className="space-y-1 text-sm">
            <div className="flex justify-between text-stone-400">
              <span>Subtotal</span><span>{formatSoles(ticket.subtotal)}</span>
            </div>
            <div className="flex justify-between text-stone-400">
              <span>IGV ({(config.tasaIgv * 100).toFixed(1)}%)</span>
              <span>{formatSoles(ticket.igv)}</span>
            </div>
            <div className="flex justify-between font-bold text-stone-100 text-base">
              <span>TOTAL</span><span>{formatSoles(ticket.total)}</span>
            </div>
            {ticket.metodoPagoSecundarioNombre && ticket.montoMetodoPrimario ? (
          <>
            <div className="flex justify-between text-stone-500 text-xs">
              <span>{ticket.metodoPagoNombre}</span>
              <span>{formatSoles(ticket.montoMetodoPrimario)}</span>
            </div>
            <div className="flex justify-between text-stone-500 text-xs">
              <span>{ticket.metodoPagoSecundarioNombre}</span>
              <span>{formatSoles(ticket.total - ticket.montoMetodoPrimario)}</span>
            </div>
          </>
        ) : (
          <div className="flex justify-between text-stone-500 text-xs">
            <span>Método de pago</span><span>{ticket.metodoPagoNombre}</span>
          </div>
        )}
          </div>

          {/* Acciones */}
          <div className="flex gap-2 pt-1">
            <button
              onClick={() => window.print()}
              className="flex-1 py-3 rounded-lg font-semibold text-stone-300 bg-stone-700 hover:bg-stone-600 transition-colors text-sm"
            >
              🖨️ Imprimir
            </button>
            <button
              onClick={onClose}
              className="flex-1 py-3 rounded-lg font-bold text-white text-sm transition-colors bg-[#c2622a]"
            >
              Nueva venta
            </button>
          </div>

        </div>
      </div>
    </>
  )
}
