import { useState, useEffect, useCallback, startTransition } from 'react'
import { PeriodoSelector, type Periodo } from '../components/PeriodoSelector'
import { KPICard } from '../components/KPICard'
import { VentasPorDia } from '../components/VentasPorDia'
import { VentasPorMetodoPago } from '../components/VentasPorMetodoPago'
import { CierreCaja } from '../components/CierreCaja'
import { api } from '../api/client'
import type { VentasResumenDto, VentasPorMetodoPagoDto, VentasPorDiaDto, TurnoActivoDto } from '../types'

const fmtS = (n: number) => `S/ ${n.toFixed(2)}`
const hoy = () => new Date().toLocaleDateString('es-PE', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })

export function DashboardPage() {
  const [periodo, setPeriodo] = useState<Periodo>('dia')
  const [resumen, setResumen] = useState<VentasResumenDto | null>(null)
  const [metodos, setMetodos] = useState<VentasPorMetodoPagoDto[]>([])
  const [porDia, setPorDia] = useState<VentasPorDiaDto[]>([])
  const [stockCount, setStockCount] = useState(0)
  const [turno, setTurno] = useState<TurnoActivoDto | null>(null)
  const [err, setErr] = useState('')

  const cargar = useCallback(async () => {
    setErr('')
    try {
      const [r, m, d] = await Promise.all([
        api.ventasResumen(periodo),
        api.ventasPorMetodoPago(periodo),
        api.ventasPorDia(periodo),
      ])
      setResumen(r)
      setMetodos(m || [])
      setPorDia(d || [])
    } catch { setErr('No se pudo conectar con la API en localhost:5138') }
  }, [periodo])

  const cargarExtras = useCallback(async () => {
    try {
      const [s, t] = await Promise.all([api.stockBajo(), api.turnoActivo()])
      setStockCount((s || []).length); setTurno(t)
    } catch { /* silencioso */ }
  }, [])

  useEffect(() => { startTransition(() => { void cargar() }) }, [cargar])
  useEffect(() => { startTransition(() => { void cargarExtras() }) }, [cargarExtras])

  const subPeriodo = periodo === 'dia' ? 'hoy' : periodo === 'semana' ? 'últimos 7 días' : 'este mes'

  return (
    <div className="page">
      <div className="page-header">
        <h1 className="page-title">Panel de Control</h1>
        <div className="page-meta">{hoy()}</div>
      </div>

      <PeriodoSelector value={periodo} onChange={setPeriodo} />

      {err && <div className="error-banner">{err}</div>}

      <div className="kpi-grid">
        <KPICard label="Ventas" value={resumen ? fmtS(resumen.totalVentas) : '—'} icon="💰" sub={subPeriodo} />
        <KPICard label="Transacciones" value={resumen ? String(resumen.numTransacciones) : '—'} icon="📊" sub={subPeriodo} />
        <KPICard label="Ticket Promedio" value={resumen ? fmtS(resumen.ticketPromedio) : '—'} icon="🧾" sub="por transacción" />
        <KPICard label="Alertas de Stock" value={String(stockCount)} icon="⚠️" sub={stockCount === 0 ? 'todo normal' : 'bajo mínimo'} />
      </div>

      <div className="chart-row">
        <VentasPorDia periodo={periodo} porDia={porDia} porHora={[]} />
        <VentasPorMetodoPago data={metodos} />
      </div>

      <CierreCaja turno={turno} onRefresh={cargarExtras} />
    </div>
  )
}
