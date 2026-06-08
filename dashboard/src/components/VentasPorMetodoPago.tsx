import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import type { VentasPorMetodoPagoDto } from '../types'
import { chartPalette } from '../lib/tokens'

const COLORS = chartPalette

interface Props { data: VentasPorMetodoPagoDto[] }

export function VentasPorMetodoPago({ data }: Props) {
  return (
    <div className="card">
      <div className="card-title">Métodos de Pago</div>
      {data.length === 0
        ? <div className="empty-state">Sin datos</div>
        : <ResponsiveContainer width="100%" height={240}>
            <PieChart>
              <Pie data={data} dataKey="totalVentas" nameKey="metodoPago" cx="50%" cy="50%" outerRadius={80}
                label={({ name, percent }: { name?: string; percent?: number }) => `${name ?? ''} ${((percent || 0)*100).toFixed(0)}%`}
                labelLine={false}>
                {data.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
              </Pie>
              <Tooltip formatter={(v: number | string | readonly (string | number)[] | null | undefined) => `S/ ${Number(v ?? 0).toFixed(2)}`} />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
      }
    </div>
  )
}
