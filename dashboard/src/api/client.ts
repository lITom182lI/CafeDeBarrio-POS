import type {
  VentasResumenDto, VentasPorMetodoPagoDto, VentasPorDiaDto,
  StockBajoDto, TurnoActivoDto, CerrarTurnoResultDto,
  ProductoDto, TransaccionListItemDto, TransaccionDetalleDto,
  CategoriaCafeDto, ProductoFormData, OperadorDto
} from '../types';

const SEDE = 1;
async function get<T>(path: string): Promise<T> {
  const token = localStorage.getItem('token')
  const headers: HeadersInit = {}
  if (token) headers['Authorization'] = `Bearer ${token}`
  const r = await fetch(path, { headers })
  if (r.status === 401) {
    localStorage.removeItem('token')
    window.location.href = '/login'
    throw new Error('Sesión expirada')
  }
  if (!r.ok) throw new Error(`HTTP ${r.status}: ${path}`)
  return r.json() as Promise<T>
}
async function post<T>(path: string, body: unknown): Promise<T> {
  const token = localStorage.getItem('token')
  const r = await fetch(path, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: JSON.stringify(body)
  })
  if (r.status === 401) {
    localStorage.removeItem('token')
    window.location.href = '/login'
    throw new Error('Sesión expirada')
  }
  if (!r.ok) throw new Error(`HTTP ${r.status}: ${path}`)
  return r.json() as Promise<T>
}

async function put(path: string, body: unknown): Promise<void> {
  const token = localStorage.getItem('token')
  const r = await fetch(path, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: JSON.stringify(body)
  })
  if (r.status === 401) {
    localStorage.removeItem('token')
    window.location.href = '/login'
    throw new Error('Sesión expirada')
  }
  if (!r.ok) throw new Error(`HTTP ${r.status}: ${path}`)
}

export const api = {
  ventasResumen:       (p: string) => get<VentasResumenDto>(`/api/reportes/ventas-resumen?sedeId=${SEDE}&periodo=${p}`),
  ventasPorMetodoPago: (p: string) => get<VentasPorMetodoPagoDto[]>(`/api/reportes/ventas-por-metodo-pago?sedeId=${SEDE}&periodo=${p}`),
  ventasPorDia:        (p: string) => get<VentasPorDiaDto[]>(`/api/reportes/ventas-por-dia?sedeId=${SEDE}&periodo=${p}`),
  categorias:       () => get<CategoriaCafeDto[]>('/api/categorias'),
  crearProducto:    (data: ProductoFormData) => post<number>('/api/productos', data),
  actualizarProducto: (id: number, data: ProductoFormData) =>
    put(`/api/productos/${id}`, { productoId: id, ...data }),
  stockBajo:           ()          => get<StockBajoDto[]>(`/api/reportes/stock-bajo`),
  productos:           ()          => get<any>(`/api/productos?pageSize=1000`).then(r => r.items ? (r.items as ProductoDto[]) : (r as ProductoDto[])),
  transacciones:       ()          => get<TransaccionListItemDto[]>(`/api/transacciones?sedeId=${SEDE}`),
  cambiarContrasena: (currentPassword: string, newPassword: string) =>
    put('/api/auth/change-password', { currentPassword, newPassword }),
  operadores:       () => get<OperadorDto[]>('/api/operadores'),
  crearOperador:    (nombre: string, pin: string) =>
    post<number>('/api/operadores', { nombre, pin }),
  actualizarOperador: (id: number, data: { nombre: string; activo: boolean; nuevoPin?: string }) =>
    put(`/api/operadores/${id}`, { operadorId: id, ...data }),
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
