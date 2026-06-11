import { config } from '../config'
import type { TicketData } from '../types'
import { formatSoles } from '../utils'
import { Printer, CheckCircle2, CloudLightning } from 'lucide-react'

interface Props {
  ticket: TicketData
  onClose: () => void
}

export default function TicketModal({ ticket, onClose }: Props) {
  const printTicket = () => {
    window.print()
  }

  const igvRate = (config.tasaIgv * 100).toFixed(0)

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-xs p-4 overflow-y-auto animate-fadeIn">
      <div className="bg-white rounded-[24px] shadow-lg w-full max-w-sm overflow-hidden my-8 border border-[#E2E8F0] flex flex-col">
        
        {/* State Banner */}
        <div className={`px-4 py-3 flex items-center gap-2 text-white ${
          ticket.offline ? 'bg-[#7C2D12]' : 'bg-[#10B981]'
        }`}>
          {ticket.offline ? (
            <>
              <CloudLightning size={16} />
              <span className="font-extrabold text-[11px] uppercase tracking-wider">Guardado en espera local</span>
            </>
          ) : (
            <>
              <CheckCircle2 size={16} />
              <span className="font-extrabold text-[11px] uppercase tracking-wider">Comanda enviada a Cocina</span>
            </>
          )}
        </div>

        {/* Paper Ticket Visual Mock */}
        <div className="p-6 bg-[#F8FAFC] border-b border-dashed border-[#E2E8F0]">
          <div className="bg-white p-4 shadow-xs rounded-lg border border-[#E2E8F0] font-mono text-[11px] text-stone-800 space-y-4">
            
            {/* Header */}
            <div className="text-center space-y-1">
              <h3 className="font-extrabold text-xs uppercase tracking-wide">CAFÉ DE BARRIO S.A.C.</h3>
              <p className="text-[10px] text-stone-550 leading-relaxed">
                Av. Reducto 1234, Barranco<br />
                Lima, Perú • RUC: 20123456789
              </p>
            </div>

            <div className="border-t border-dashed border-stone-300 my-2" />

            {/* Ticket Metadata state */}
            <div className="space-y-0.5 text-[10px]">
              <p><span className="font-bold">Comprobante:</span> {ticket.comprobante ? `${ticket.comprobante.tipoDocumento} - ${ticket.comprobante.numeroDocumento}` : 'Ticket de Venta Simple'}</p>
              <p><span className="font-bold">Adquiriente:</span> {ticket.comprobante ? ticket.comprobante.razonSocial : 'Público General'}</p>
              <p><span className="font-bold">Fecha / Hora:</span> {new Date(ticket.fechaHora).toLocaleString()}</p>
              <p><span className="font-bold">Nro Int:</span> {ticket.offline ? `L-${ticket.localId}` : `S-${ticket.transaccionId}`}</p>
            </div>

            <div className="border-t border-dashed border-stone-300 my-2" />

            {/* Transacted items list details */}
            <div className="space-y-1.5 py-1">
              {ticket.items.map(item => (
                <div key={item.productoId} className="flex justify-between items-start">
                  <div className="flex-1 pr-2">
                    <p className="font-bold leading-tight uppercase">{item.nombre}</p>
                    <p className="text-[10px] text-stone-500">{item.cantidad} x {formatSoles(item.precio)}</p>
                  </div>
                  <span className="font-bold">{formatSoles(item.precio * item.cantidad)}</span>
                </div>
              ))}
            </div>

            <div className="border-t border-dashed border-stone-300 my-2" />

            {/* Math aggregate calculations fields */}
            <div className="space-y-0.5 text-right font-semibold">
              <div className="flex justify-between">
                <span>Subtotal Gravado S/.:</span>
                <span>{formatSoles(ticket.subtotal)}</span>
              </div>
              <div className="flex justify-between">
                <span>IGV ({igvRate}%) S/.:</span>
                <span>{formatSoles(ticket.igv)}</span>
              </div>
              <div className="flex justify-between font-extrabold text-xs pt-1 border-t border-stone-200">
                <span>TOTAL NETO S/.:</span>
                <span>{formatSoles(ticket.total)}</span>
              </div>
            </div>

            {/* Split double payment detail */}
            <div className="text-[9px] text-stone-500 pt-3">
              <p>MÉD. PAGO: {ticket.metodoPagoNombre}</p>
              {ticket.metodoPagoSecundarioNombre && ticket.montoMetodoPrimario && (
                <p className="font-bold mt-0.5">
                  → Multiclase: {ticket.metodoPagoNombre} (S/. {ticket.montoMetodoPrimario.toFixed(2)}) + {ticket.metodoPagoSecundarioNombre} (S/. {(ticket.total - ticket.montoMetodoPrimario).toFixed(2)})
                </p>
              )}
            </div>

            <div className="border-t border-dashed border-stone-300 pt-3 text-center text-[10px]">
              <p className="font-bold uppercase">¡Disfruta tu Café de Barrio!</p>
              <p className="text-[9px] text-stone-500 mt-0.5">La comanda se procesó en la Sede Nro {config.sedeId}</p>
            </div>

          </div>
        </div>

        {/* Modal Actions */}
        <div className="bg-[#F8FAFC] px-6 py-4 flex gap-2 justify-between border-t border-[#E2E8F0]">
          <button
            onClick={printTicket}
            className="px-4 py-2 border border-[#E2E8F0] bg-white rounded-lg text-xs font-bold text-[#334155]/80 hover:bg-[#F8FAFC] flex items-center gap-2 transition cursor-pointer"
          >
            <Printer size={14} />
            <span>Imprimir Ticket</span>
          </button>
          <button
            onClick={onClose}
            className="px-6 py-2 bg-[#7C2D12] hover:bg-[#6b250e] text-white font-extrabold rounded-lg text-xs transition shadow-2xs cursor-pointer"
          >
            Nueva Venta
          </button>
        </div>

      </div>
    </div>
  )
}
