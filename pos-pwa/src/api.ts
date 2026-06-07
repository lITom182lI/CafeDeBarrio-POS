import { config } from './config'
import type {
  ProductoDto, CategoriaDto, MetodoPagoDto,
  OperadorDto, OperadorLoginDto, CreateTransaccionRequest
} from './types'

// Error distinguible para modo offline
export class OfflineError extends Error {
  constructor() { super('Sin conexión con el servidor') }
}

// fetch con timeout configurable
async function apiFetch<T>(
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
        ...options?.headers,
      },
    })

    if (!res.ok) {
      const text = await res.text().catch(() => res.statusText)
      throw new Error(`API ${res.status}: ${text}`)
    }

    // 204 No Content
    if (res.status === 204) return undefined as T

    return res.json() as Promise<T>
  } catch (err) {
    if (err instanceof Error && (
      err.name === 'AbortError' ||
      err.message.includes('fetch') ||
      err.message.includes('Failed') ||
      err.message.includes('NetworkError')
    )) {
      throw new OfflineError()
    }
    throw err
  } finally {
    clearTimeout(timer)
  }
}

// ─── Catálogo ────────────────────────────────────────────────────────────

interface PaginatedResult<T> {
  items: T[]
  totalCount: number
}

export async function getProductos(): Promise<ProductoDto[]> {
  const result = await apiFetch<PaginatedResult<ProductoDto> | ProductoDto[]>('/api/productos?pageSize=1000')
  return Array.isArray(result) ? result : result.items ?? []
}

export async function getCategorias(): Promise<CategoriaDto[]> {
  return apiFetch<CategoriaDto[]>('/api/categorias')
}

export async function getMetodosPago(): Promise<MetodoPagoDto[]> {
  return apiFetch<MetodoPagoDto[]>('/api/metodos-pago')
}

// ─── Operadores ───────────────────────────────────────────────────────────

export async function getOperadores(): Promise<OperadorDto[]> {
  return apiFetch<OperadorDto[]>('/api/operadores')
}

// PIN usa Argon2id — puede tardar hasta 3s, timeout extendido
export async function validarPin(
  operadorId: number,
  pin: string
): Promise<OperadorLoginDto | null> {
  try {
    return await apiFetch<OperadorLoginDto>(
      '/api/operadores/validar-pin',
      {
        method: 'POST',
        body: JSON.stringify({ operadorId, pin }),
      },
      15000 // 15s por Argon2id
    )
  } catch (err) {
    if (err instanceof Error && err.message.includes('401')) return null
    throw err
  }
}

// ─── Transacciones ────────────────────────────────────────────────────────

export async function crearTransaccion(
  request: CreateTransaccionRequest
): Promise<number> {
  return apiFetch<number>('/api/transacciones', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

// ─── Disponibilidad de la API ─────────────────────────────────────────────

export async function checkOnline(): Promise<boolean> {
  try {
    await apiFetch<MetodoPagoDto[]>('/api/metodos-pago', undefined, 4000)
    return true
  } catch {
    return false
  }
}
