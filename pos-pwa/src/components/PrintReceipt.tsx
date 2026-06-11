import type { TicketData } from '../types'
import { config } from '../config'

interface Props {
  ticket: TicketData
}

function buildReceipt(ticket: TicketData): string {
  const W    = 32
  const SEP  = '-'.repeat(W)
  const SEP2 = '='.repeat(W)

  const center = (s: string) => {
    const pad = Math.max(0, Math.floor((W - s.length) / 2))
    return ' '.repeat(pad) + s
  }
  const padR = (s: string, w: number) => s.length >= w ? s.slice(0, w) : s.padEnd(w)
  const padL = (s: string, w: number) => s.length >= w ? s.slice(0, w) : s.padStart(w)

  const fecha = new Date(ticket.fechaHora).toLocaleString('es-PE', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  })
  const igvPct = `${(config.tasaIgv * 100).toFixed(1)}%`

  const lines: string[] = [
    SEP2,
    center('CAFE DE BARRIO'),
    SEP2,
    fecha,
    ticket.offline
      ? `ID: ${ticket.localId?.slice(0, 20) ?? '-'}`
      : `Transaccion #${ticket.transaccionId}`,
  ]

  if (ticket.comprobante) {
    lines.push('')
    lines.push(center('-- BOLETA NOMINADA --'))
    lines.push(SEP)
    lines.push(`${ticket.comprobante.tipoDocumento}: ${ticket.comprobante.numeroDocumento}`)
    const rs = ticket.comprobante.razonSocial ?? ''
    lines.push(rs.length > W ? rs.slice(0, W - 1) + '.' : rs)
    lines.push(SEP)
  }

  lines.push('')
  lines.push(padR('Producto', 18) + padL('Cant', 5) + padL('Total', 9))
  lines.push(SEP)

  for (const item of ticket.items) {
    const nombre = padR(item.nombre.toUpperCase(), 18)
    const cant   = padL(String(item.cantidad), 5)
    const tot    = padL(`S/${(item.precio * item.cantidad).toFixed(2)}`, 9)
    lines.push(`${nombre}${cant}${tot}`)
    if (item.cantidad > 1) lines.push(`  x S/${item.precio.toFixed(2)} c/u`)
  }

  lines.push(SEP)
  lines.push(padR('Subtotal:', 23)          + padL(`S/${ticket.subtotal.toFixed(2)}`, 9))
  lines.push(padR(`IGV (${igvPct}):`, 23)   + padL(`S/${ticket.igv.toFixed(2)}`, 9))
  lines.push(padR('TOTAL:', 23)             + padL(`S/${ticket.total.toFixed(2)}`, 9))
  lines.push(SEP)
  if (ticket.metodoPagoSecundarioNombre && ticket.montoMetodoPrimario) {
    lines.push(padR(`Pago 1 (${ticket.metodoPagoNombre}):`, 23) + padL(`S/${ticket.montoMetodoPrimario.toFixed(2)}`, 9))
    lines.push(padR(`Pago 2 (${ticket.metodoPagoSecundarioNombre}):`, 23) + padL(`S/${(ticket.total - ticket.montoMetodoPrimario).toFixed(2)}`, 9))
  } else {
    lines.push(`Pago: ${ticket.metodoPagoNombre}`)
  }
  if (ticket.offline) lines.push('*** PENDIENTE DE SINCRONIZAR ***')
  lines.push(SEP2)
  lines.push(center('Gracias por su visita!'))
  lines.push(SEP2)
  lines.push('')
  lines.push('')

  return lines.join('\n')
}

export default function PrintReceipt({ ticket }: Props) {
  return (
    <div
      id="thermal-receipt"
      className="hidden print:block font-mono text-[11px] w-[72mm] text-black bg-white"
    >
      <pre className="whitespace-pre m-0 leading-[1.35]">{buildReceipt(ticket)}</pre>
    </div>
  )
}
