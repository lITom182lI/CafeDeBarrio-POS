import type {
  VentasResumenDto, VentasPorMetodoPagoDto, VentasPorDiaDto,
  StockBajoDto, TurnoActivoDto, CerrarTurnoResultDto,
  ProductoDto, TransaccionListItemDto, TransaccionDetalleDto
} from '../types';

const SEDE = 1;
async function get<T>(path: string): Promise<T> {
  const r = await fetch(path);
  if (!r.ok) throw new Error(`HTTP ${r.status}: ${path}`);
  return r.json() as Promise<T>;
}

export const api = {
  ventasResumen:       (p: string) => get<VentasResumenDto>(`/api/reportes/ventas-resumen?sedeId=${SEDE}&periodo=${p}`),
  ventasPorMetodoPago: (p: string) => get<VentasPorMetodoPagoDto[]>(`/api/reportes/ventas-por-metodo-pago?sedeId=${SEDE}&periodo=${p}`),
  ventasPorDia:        (p: string) => get<VentasPorDiaDto[]>(`/api/reportes/ventas-por-dia?sedeId=${SEDE}&periodo=${p}`),
  stockBajo:           ()          => get<StockBajoDto[]>(`/api/reportes/stock-bajo`),
  productos:           ()          => get<any>(`/api/productos?pageSize=1000`).then(r => r.items ? (r.items as ProductoDto[]) : (r as ProductoDto[])),
  transacciones:       ()          => get<TransaccionListItemDto[]>(`/api/transacciones?sedeId=${SEDE}`),
  transaccionDetalle:  (id: number)=> get<TransaccionDetalleDto>(`/api/transacciones/${id}`),
  turnoActivo:         ()          => get<TurnoActivoDto | null>(`/api/turnos/activo?sedeId=${SEDE}`),
  abrirTurno: (operadorId: number, montoApertura: number) =>
    fetch('/api/turnos/abrir', { method:'POST', headers:{'Content-Type':'application/json'},
      body: JSON.stringify({ sedeId: SEDE, operadorId, montoApertura }) })
    .then(r => { if (!r.ok) throw new Error('No se pudo abrir el turno'); return r.json() as Promise<{turnoId:number}>; }),
  cerrarTurno: (turnoId: number, montoEfectivoCierto: number, observaciones?: string) =>
    fetch(`/api/turnos/${turnoId}/cerrar`, { method:'PUT', headers:{'Content-Type':'application/json'},
      body: JSON.stringify({ montoEfectivoCierto, observaciones }) })
    .then(r => { if (!r.ok) throw new Error('No se pudo cerrar el turno'); return r.json() as Promise<CerrarTurnoResultDto>; }),
};
