// ─── Respuestas de la API ──────────────────────────────────────────────────

export interface ProductoDto {
  productoId: number
  nombre: string
  descripcion?: string
  precio: number
  cantidadDisponible: number
  categoriaNombre: string | null
  activo: boolean
  seguimientoInventario: boolean
}

export interface CategoriaDto {
  categoriaId: number
  nombre: string
  codigo: string
}

export interface MetodoPagoDto {
  metodoPagoId: number
  nombre: string
}

export interface OperadorDto {
  operadorId: number
  nombre: string
  activo: boolean
}

export interface OperadorLoginDto {
  operadorId: number
  nombre: string
  token: string
}

export interface TurnoActivoDto {
  turnoId: number
  sedeId: number
  operadorId: number
  fechaApertura: string
  montoApertura: number
  estado: string
}

// ─── Estado local del carrito ─────────────────────────────────────────────

export interface CartItem {
  productoId: number
  nombre: string
  precio: number
  cantidad: number
}

// ─── Comprobante nominado (boleta con nombre) ─────────────────────────────

export interface ComprobanteData {
  tipoDocumento: 'DNI' | 'RUC' | 'CE' | 'Pasaporte'
  numeroDocumento: string
  razonSocial?: string
}

// ─── Request al API para crear transacción ────────────────────────────────

export interface ItemRequest {
  productoId: number
  cantidad: number
}

export interface CreateTransaccionRequest {
  sedeId: number
  clienteId: number | null
  metodoPagoId: number
  items: ItemRequest[]
  operadorId?: number | null
  tipoDocumento?: string | null
  numeroDocumento?: string | null
  razonSocial?: string | null
  metodoPagoSecundarioId?: number | null
  montoMetodoPrimario?: number | null
}

// ─── Cola offline (IndexedDB) ─────────────────────────────────────────────

export interface PendingTransaction {
  localId: string           // UUID generado localmente
  sedeId: number
  metodoPagoId: number
  metodoPagoNombre: string  // para mostrar en el ticket offline
  operadorId: number | null
  items: ItemRequest[]
  cartSnapshot: CartItem[]  // para imprimir el ticket
  subtotal: number
  igv: number
  total: number
  fechaLocal: string        // ISO string
  tipoDocumento: string | null
  numeroDocumento: string | null
  razonSocial: string | null
  sincronizada: number
  transaccionIdServidor: number | null
  error: string | null
}

// ─── Estado de la sesión del operador ────────────────────────────────────

export interface OperadorSession {
  operadorId: number
  nombre: string
  token: string | null
}

// ─── Totales calculados del carrito ──────────────────────────────────────

export interface Totales {
  subtotal: number
  igv: number
  total: number
}

// ─── Configuración de la app (variables de entorno) ──────────────────────

export interface AppConfig {
  apiUrl: string
  sedeId: number
  tasaIgv: number   // 0.105 para MYPE restaurante 2026
}

// ─── Datos del ticket post-venta ──────────────────────────────────────────

export interface TicketData {
  transaccionId?: number
  localId?: string
  items: CartItem[]
  subtotal: number
  igv: number
  total: number
  metodoPagoNombre: string
  comprobante: ComprobanteData | null
  fechaHora: string
  offline: boolean
  metodoPagoSecundarioNombre?: string
  montoMetodoPrimario?: number
}
