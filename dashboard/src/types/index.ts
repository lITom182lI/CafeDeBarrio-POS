export interface VentasResumenDto { totalVentas: number; numTransacciones: number; ticketPromedio: number; desde: string; hasta: string; }
export interface VentasPorMetodoPagoDto { metodoPago: string; totalVentas: number; numTransacciones: number; }
export interface TopProductoDto { productoId: number; nombre: string; cantidadVendida: number; totalVentas: number; }
export interface VentasPorHoraDto { hora: number; totalVentas: number; numTransacciones: number; }
export interface VentasPorDiaDto { fecha: string; totalVentas: number; numTransacciones: number; }
export interface StockBajoDto { productoId: number; nombre: string; cantidadDisponible: number; stockMinimo: number; unidadMedida: string; }
export interface TurnoActivoDto { turnoId: number; nombreOperador: string; fechaApertura: string; montoApertura: number; estado: string; }
export interface CerrarTurnoResultDto { totalEfectivoSistema: number; montoEfectivoCierto: number; diferencia: number; }
export interface ProductoDto {
  productoId: number; nombre: string; precio: number;
  cantidadDisponible: number; stockMinimo: number; unidadMedida: string;
  categoriaId: number; categoriaNombre?: string; activo: boolean;
  descripcion?: string;
  costo?: number;
  seguimientoInventario?: boolean;
  esMayorista?: boolean;
}
export interface TransaccionListItemDto { transaccionId: number; clienteNombre: string; total: number; fecha: string; metodoPago: string; anulada: boolean; operadorNombre?: string; tipoDocumento?: string; numeroDocumento?: string; razonSocial?: string; metodoPagoSecundario?: string; motivoAnulacion?: string; }
export interface TransaccionDetalleDto { transaccionId: number; clienteNombre: string; total: number; subtotal: number; igv: number; fecha: string; metodoPago: string; anulada: boolean; items: DetalleItemDto[]; operadorNombre?: string; tipoDocumento?: string; numeroDocumento?: string; razonSocial?: string; metodoPagoSecundario?: string; montoMetodoPrimario?: number; motivoAnulacion?: string; }
export interface DetalleItemDto { nombreProducto: string; cantidad: number; precioUnitario: number; subtotalLinea: number; }
export interface CategoriaCafeDto {
  categoriaId: number
  codigo: string
  nombre: string
}

export interface ProductoFormData {
  nombre: string
  descripcion: string
  costo: number
  precio: number
  cantidadDisponible: number
  stockMinimo: number
  unidadMedida: string
  seguimientoInventario: boolean
  esMayorista: boolean
  categoriaId: number
  activo: boolean
}

export interface OperadorDto {
  operadorId: number
  nombre: string
  activo: boolean
  eliminado: boolean
}

export interface OperadorFormData {
  nombre: string
  activo: boolean
  nuevoPin: string
}

export interface TurnoCerradoDto {
  turnoId: number;
  operadorId: number;
  operadorNombre: string;
  fechaApertura: string;
  fechaCierre: string;
  montoApertura: number;
  montoEfectivoCierto: number;
  totalEfectivoSistema: number;
  diferencia: number;
  estadoCierre: string;
  observaciones?: string;
}
