import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import type { VentasPorDiaDto, VentasPorHoraDto } from '../types'
import type { Periodo } from './PeriodoSelector'

interface Props { periodo: Periodo; porDia: VentasPorDiaDto[]; porHora: VentasPorHoraDto[] }

const fmtDia  = (s: string) => new Date(s).toLocaleDateString('es-PE', { weekday:'short', day:'numeric' })
const fmtHora = (h: number) => `${String(h).padStart(2,'0')}h`

export function VentasPorDia({ periodo, porDia, porHora }: Props) {
  const data = periodo === 'dia'
    ? porHora.map(d => ({ label: fmtHora(d.hora), value: d.totalVentas }))
    : porDia.map(d => ({ label: fmtDia(d.fecha),  value: d.totalVentas }))

  const titulo = periodo === 'dia' ? 'Ventas por Hora' : 'Ventas por Día'

  return (
    <div className="card">
      <div className="card-title">{titulo}</div>
      {data.length === 0
        ? <div className="empty-state">Sin ventas en este período</div>
        : <ResponsiveContainer width="100%" height={240}>
            <BarChart data={data} margin={{ left: 8, right: 8, bottom: 0, top: 4 }}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
              <XAxis dataKey="label" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip formatter={(v: any) => [`S/ ${Number(v).toFixed(2)}`, 'Ventas']} />
              <Bar dataKey="value" fill="#c2622a" radius={[4,4,0,0]} name="Ventas" />
            </BarChart>
          </ResponsiveContainer>
      }
    </div>
  )
}
