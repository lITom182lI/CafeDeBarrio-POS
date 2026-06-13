import { config } from '../config'
import type {
  ProductoDto, CategoriaDto, MetodoPagoDto,
  OperadorDto, OperadorLoginDto, CreateTransaccionRequest
} from '../types'

interface PaginatedResult<T> { items: T[]; totalCount: number }

export class OfflineError extends Error {
  constructor() { super('Sin conexión con el servidor') }
}

export class CafeBarrioPosAdapter {
  private _token: string | null = null

  setToken(token: string | null): void {
    this._token = token
  }

  private async apiFetch<T>(
    path: string,
    options?: RequestInit,
    timeoutMs = 8000
  ): Promise<T> {
    const controller = new AbortController()
    const timer = setTimeout(() => controller.abort(), timeoutMs)

    try {
      const res = await fetch(`${config.apiUrl}${path}`, {
        ...options,
        signal: controller.signal,
        headers: {
          'Content-Type': 'application/json',
          ...(this._token ? { Authorization: `Bearer ${this._token}` } : {}),
          ...options?.headers,
        },
      })

      if (!res.ok) {
        const text = await res.text().catch(() => res.statusText)
        throw new Error(`API ${res.status}: ${text}`)
      }

      if (res.status === 204) return undefined as T
      return res.json() as Promise<T>
    } catch (err) {
      if (
        err instanceof Error && (
          err.name === 'AbortError' ||
          err.message.includes('fetch') ||
          err.message.includes('Failed') ||
          err.message.includes('NetworkError')
        )
      ) {
        throw new OfflineError()
      }
      throw err
    } finally {
      clearTimeout(timer)
    }
  }

  async getProductos(): Promise<ProductoDto[]> {
    let allProducts: ProductoDto[] = []
    let currentPage = 1
    const pageSize = 100
    const MAX_PAGES = 50
    while (currentPage <= MAX_PAGES) {
      const r = await this.apiFetch<PaginatedResult<ProductoDto> | ProductoDto[]>(
        `/api/productos?pageNumber=${currentPage}&pageSize=${pageSize}`
      )
      const items = Array.isArray(r) ? r : (r as PaginatedResult<ProductoDto>)?.items ?? []
      allProducts = allProducts.concat(items)
      if (items.length < pageSize || Array.isArray(r)) break
      currentPage++
    }
    return allProducts
  }

  getCategorias  = () => this.apiFetch<CategoriaDto[]>('/api/categorias')
  getMetodosPago = () => this.apiFetch<MetodoPagoDto[]>('/api/metodos-pago')
  getOperadores  = () => this.apiFetch<OperadorDto[]>('/api/operadores/activos')
  getTasas       = (sedeId: number) =>
    this.apiFetch<{ tasaIgv: number }>(`/api/configuracion/tasas?sedeId=${sedeId}`)

  async validarPin(operadorId: number, pin: string): Promise<OperadorLoginDto | null> {
    try {
      const result = await this.apiFetch<OperadorLoginDto>(
        '/api/operadores/validar-pin',
        {
          method: 'POST',
          body: JSON.stringify({ operadorId, pin }),
          headers: { 'X-Operator-Id': String(operadorId) }
        },
        15000
      )
      if (result && result.token) {
        localStorage.setItem('pos_auth_token', result.token)
      }
      return result
    } catch (err) {
      if (err instanceof Error && err.message.includes('401')) return null
      throw err
    }
  }

  turnoActivo = () =>
    this.apiFetch<import('../types').TurnoActivoDto | null>(`/api/turnos/activo?sedeId=${config.sedeId}&t=${Date.now()}`, { cache: 'no-store' })

  cerrarTurno = (turnoId: number, montoEfectivoCierto: number, observaciones: string) => {
    localStorage.removeItem('pos_auth_token')
    return this.apiFetch<unknown>(`/api/turnos/${turnoId}/cerrar`, {
      method: 'PUT',
      body: JSON.stringify({ montoEfectivoCierto, observaciones }),
    })
  }

  crearTransaccion = (request: CreateTransaccionRequest) =>
    this.apiFetch<number>('/api/transacciones', {
      method: 'POST',
      body: JSON.stringify({
        ...request,
        idempotencyKey: request.idempotencyKey ?? crypto.randomUUID(),
      }),
    })

  async checkOnline(): Promise<boolean> {
    try {
      await this.apiFetch<MetodoPagoDto[]>('/api/metodos-pago', undefined, 4000)
      return true
    } catch {
      return false
    }
  }
}

export const posAdapter = new CafeBarrioPosAdapter()
