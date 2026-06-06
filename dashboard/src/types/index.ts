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
}
export interface TransaccionListItemDto { transaccionId: number; clienteNombre: string; total: number; fecha: string; metodoPago: string; anulada: boolean; }
export interface TransaccionDetalleDto { transaccionId: number; clienteNombre: string; total: number; subtotal: number; igv: number; fecha: string; metodoPago: string; anulada: boolean; items: DetalleItemDto[]; }
export interface DetalleItemDto { nombreProducto: string; cantidad: number; precioUnitario: number; subtotalLinea: number; }
