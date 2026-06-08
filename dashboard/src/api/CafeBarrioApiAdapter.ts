import type {
  VentasResumenDto, VentasPorMetodoPagoDto, VentasPorDiaDto,
  StockBajoDto, TurnoActivoDto, CerrarTurnoResultDto,
  ProductoDto, TransaccionListItemDto, TransaccionDetalleDto,
  CategoriaCafeDto, ProductoFormData, OperadorDto
} from '../types'

interface PaginatedResult<T> { items: T[]; totalCount: number }

const SEDE = 1

export class CafeBarrioApiAdapter {
  private get token(): string | null {
    return localStorage.getItem('token')
  }

  private get authHeaders(): HeadersInit {
    return {
      'Content-Type': 'application/json',
      ...(this.token ? { Authorization: `Bearer ${this.token}` } : {})
    }
  }

  private async _get<T>(path: string): Promise<T> {
    const r = await fetch(path, { headers: this.authHeaders })
    if (r.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
      throw new Error('Sesión expirada')
    }
    if (!r.ok) throw new Error(`HTTP ${r.status}: ${path}`)
    const text = await r.text()
    if (!text) return null as unknown as T
    const parsed = JSON.parse(text)
    // Desenvuelve Result<T> del backend ({ isSuccess, value, errors })
    if (parsed && typeof parsed === 'object' && 'isSuccess' in parsed && 'value' in parsed) {
      return parsed.value as T
    }
    return parsed as T
  }

  private async _post<T>(path: string, body: unknown): Promise<T> {
    const r = await fetch(path, { method: 'POST', headers: this.authHeaders, body: JSON.stringify(body) })
    if (r.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
      throw new Error('Sesión expirada')
    }
    if (!r.ok) throw new Error(`HTTP ${r.status}: ${path}`)
    const text = await r.text()
    if (!text) return null as unknown as T
    const parsed = JSON.parse(text)
    if (parsed && typeof parsed === 'object' && 'isSuccess' in parsed && 'value' in parsed) {
      return parsed.value as T
    }
    return parsed as T
  }

  private async _put(path: string, body: unknown): Promise<void> {
    const r = await fetch(path, { method: 'PUT', headers: this.authHeaders, body: JSON.stringify(body) })
    if (r.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
      throw new Error('Sesión expirada')
    }
    if (!r.ok) throw new Error(`HTTP ${r.status}: ${path}`)
  }

  ventasResumen       = (p: string) =>
    this._get<VentasResumenDto>(`/api/reportes/ventas-resumen?sedeId=${SEDE}&periodo=${p}`)
  ventasPorMetodoPago = (p: string) =>
    this._get<VentasPorMetodoPagoDto[]>(`/api/reportes/ventas-por-metodo-pago?sedeId=${SEDE}&periodo=${p}`)
      .then(r => Array.isArray(r) ? r : [])
  ventasPorDia        = (p: string) =>
    this._get<VentasPorDiaDto[]>(`/api/reportes/ventas-por-dia?sedeId=${SEDE}&periodo=${p}`)
      .then(r => Array.isArray(r) ? r : [])
  categorias          = () => this._get<CategoriaCafeDto[]>('/api/categorias').then(r => Array.isArray(r) ? r : [])
  crearProducto       = (data: ProductoFormData) => this._post<number>('/api/productos', data)
  actualizarProducto  = (id: number, data: ProductoFormData) =>
    this._put(`/api/productos/${id}`, { productoId: id, ...data })
  stockBajo           = () => this._get<StockBajoDto[]>('/api/reportes/stock-bajo').then(r => Array.isArray(r) ? r : [])
  productos           = () =>
    this._get<PaginatedResult<ProductoDto> | ProductoDto[]>('/api/productos?pageSize=1000')
      .then(r => Array.isArray(r) ? r : (r as PaginatedResult<ProductoDto>)?.items ?? [])
  transacciones       = () =>
    this._get<TransaccionListItemDto[]>(`/api/transacciones?sedeId=${SEDE}`)
      .then(r => Array.isArray(r) ? r : [])
  cambiarContrasena   = (c: string, n: string) =>
    this._put('/api/auth/change-password', { currentPassword: c, newPassword: n })
  operadores          = () => this._get<OperadorDto[]>('/api/operadores').then(r => Array.isArray(r) ? r : [])
  crearOperador       = (nombre: string, pin: string) =>
    this._post<number>('/api/operadores', { nombre, pin })
  actualizarOperador  = (id: number, data: { nombre: string; activo: boolean; nuevoPin?: string }) =>
    this._put(`/api/operadores/${id}`, { operadorId: id, ...data })
  eliminarOperador    = (id: number) =>
    fetch(`/api/operadores/${id}`, { method: 'DELETE', headers: this.authHeaders }).then(r => {
      if (r.status === 401) { localStorage.removeItem('token'); window.location.href = '/login'; throw new Error('Sesión expirada') }
      if (!r.ok) throw new Error('No se pudo eliminar')
    })
  transaccionDetalle  = (id: number) =>
    this._get<TransaccionDetalleDto>(`/api/transacciones/${id}`)
  turnoActivo         = () =>
    this._get<TurnoActivoDto | null>(`/api/turnos/activo?sedeId=${SEDE}`)
  abrirTurno          = (operadorId: number, montoApertura: number) =>
    fetch('/api/turnos/abrir', {
      method: 'POST', headers: this.authHeaders,
      body: JSON.stringify({ sedeId: SEDE, operadorId, montoApertura })
    }).then(r => {
      if (!r.ok) throw new Error('No se pudo abrir el turno')
      return r.json() as Promise<{ turnoId: number }>
    })
  cerrarTurno         = (turnoId: number, montoEfectivoCierto: number, observaciones?: string) =>
    fetch(`/api/turnos/${turnoId}/cerrar`, {
      method: 'PUT', headers: this.authHeaders,
      body: JSON.stringify({ montoEfectivoCierto, observaciones })
    }).then(r => {
      if (!r.ok) throw new Error('No se pudo cerrar el turno')
      return r.json() as Promise<CerrarTurnoResultDto>
    })
}
