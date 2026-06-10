import { useState } from 'react'
import type { TurnoActivoDto, CerrarTurnoResultDto } from '../types'
import { api } from '../api/client'

interface Props { turno: TurnoActivoDto | null; onRefresh: () => void }

const fmtS    = (n: number) => `S/ ${n.toFixed(2)}`
const fmtDate = (s: string) => new Date(s).toLocaleString('es-PE', { dateStyle:'short', timeStyle:'short' })

export function CierreCaja({ turno, onRefresh }: Props) {
  const [opId, setOpId]       = useState('1')
  const [apertura, setApertura] = useState('')
  const [fisico, setFisico]   = useState('')
  const [obs, setObs]         = useState('')
  const [resultado, setResultado] = useState<CerrarTurnoResultDto | null>(null)
  const [loading, setLoading] = useState(false)
  const [err, setErr]         = useState('')

  const abrir = async () => {
    const m = parseFloat(apertura)
    if (isNaN(m) || m < 0) { setErr('Monto de apertura inválido'); return }
    setLoading(true); setErr('')
    try { await api.abrirTurno(parseInt(opId), m); setApertura(''); onRefresh() }
    catch (e) { setErr(String(e)) }
    finally { setLoading(false) }
  }

  const cerrar = async () => {
    if (!turno) return
    const m = parseFloat(fisico)
    if (isNaN(m) || m < 0) { setErr('Monto físico inválido'); return }
    setLoading(true); setErr('')
    try {
      const res = await api.cerrarTurno(turno.turnoId, m, obs || undefined)
      setResultado(res); setFisico(''); setObs(''); onRefresh()
    }
    catch (e) { setErr(String(e)) }
    finally { setLoading(false) }
  }

  return (
    <div className="card mt-20">
      <div className="card-title">Cierre de Caja</div>
      <div className="caja-grid">
        <div>
          {resultado && (
            <div className="result-caja mb-14">
              <div className="result-row"><span>Total sistema</span><span>{fmtS(resultado.totalEfectivoSistema)}</span></div>
              <div className="result-row"><span>Monto físico</span><span>{fmtS(resultado.montoEfectivoCierto)}</span></div>
              <div className="result-row border-top pt-6 mt-4">
                <span className="fw-semibold">Diferencia</span>
                <span className={resultado.diferencia >= 0 ? 'dif-pos' : 'dif-neg'}>
                  {resultado.diferencia >= 0 ? '+' : ''}{fmtS(resultado.diferencia)}
                </span>
              </div>
            </div>
          )}
          {turno ? (
            <div className="caja-status">
              <div className="caja-status-label">Turno activo — {turno.nombreOperador}</div>
              <div className="caja-status-value">Desde {fmtDate(turno.fechaApertura)}</div>
              <div className="caja-status-value">Apertura: {fmtS(turno.montoApertura)}</div>
            </div>
          ) : (
            <div className="caja-status sin-turno">
              <div className="caja-status-label">Sin turno activo</div>
              <div className="caja-status-value text-warning-dark">Abre un turno para operar</div>
            </div>
          )}
        </div>
        <div className="caja-form">
          {turno ? (
            <>
              <input type="number" min="0" step="0.01" placeholder="Monto físico contado (S/)"
                value={fisico} onChange={e => setFisico(e.target.value)} />
              <textarea 
                className="w-full bg-slate-50 border border-slate-200 rounded-xl p-3 focus:outline-none focus:ring-2 focus:ring-slate-400"
                style={{ resize: "none" }}
                placeholder="Observaciones (opcional)"
                value={obs} onChange={e => setObs(e.target.value)} />
              {err && <span className="text-danger text-sm">{err}</span>}
              <button className="btn btn-danger" onClick={cerrar} disabled={loading}>
                {loading ? 'Cerrando...' : 'Cerrar Turno'}
              </button>
            </>
          ) : (
            <>
              <input type="number" min="1" placeholder="ID de operador"
                value={opId} onChange={e => setOpId(e.target.value)} />
              <input type="number" min="0" step="0.01" placeholder="Monto de apertura (S/)"
                value={apertura} onChange={e => setApertura(e.target.value)} />
              {err && <span className="text-danger text-sm">{err}</span>}
              <button className="btn btn-primary" onClick={abrir} disabled={loading}>
                {loading ? 'Abriendo...' : 'Abrir Turno'}
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
