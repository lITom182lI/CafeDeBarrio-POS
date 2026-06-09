import { useState, useEffect, startTransition, useRef } from "react";
import { Search, Eye, CheckCircle, XCircle, FileText } from "lucide-react";
import { api } from "../api/client";
import type { TransaccionListItemDto, TransaccionDetalleDto } from "../types";

export function Transacciones() {
  const [transacciones, setTransacciones] = useState<TransaccionListItemDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>("");
  
  // Search / Filters State
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [filterMetodo, setFilterMetodo] = useState<string>("todos");
  const [filterEstado, setFilterEstado] = useState<string>("todos");

  // Detail Modal State
  const [selectedTxId, setSelectedTxId] = useState<number | null>(null);
  const [txDetail, setTxDetail] = useState<TransaccionDetalleDto | null>(null);
  const [loadingDetail, setLoadingDetail] = useState<boolean>(false);
  const [errorDetail, setErrorDetail] = useState<string>("");
  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (selectedTxId !== null) dialogRef.current?.focus();
  }, [selectedTxId]);

  const loadTransactions = async () => {
    setLoading(true);
    setError("");
    try {
      const data = await api.transacciones();
      setTransacciones(data);
    } catch {
      setError("No se pudo obtener el historial de transacciones. Reintentando...");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    startTransition(() => {
      void loadTransactions();
    });
  }, []);

  const handleOpenDetail = async (id: number) => {
    setSelectedTxId(id);
    setLoadingDetail(true);
    setErrorDetail("");
    setTxDetail(null);
    try {
      const data = await api.transaccionDetalle(id);
      setTxDetail(data);
    } catch {
      setErrorDetail("No se pudo cargar el detalle del pedido seleccionado.");
    } finally {
      setLoadingDetail(false);
    }
  };

  const handleCloseDetail = () => {
    setSelectedTxId(null);
    setTxDetail(null);
  };

  // Filter & Search Logic
  const filteredTx = transacciones.filter((tx) => {
    const matchesSearch =
      tx.clienteNombre?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      String(tx.transaccionId).includes(searchTerm) ||
      (tx.numeroDocumento && tx.numeroDocumento.includes(searchTerm));

    const matchesMetodo =
      filterMetodo === "todos" ||
      tx.metodoPago === filterMetodo ||
      tx.metodoPagoSecundario === filterMetodo;

    const matchesEstado =
      filterEstado === "todos" ||
      (filterEstado === "anuladas" && tx.anulada) ||
      (filterEstado === "completadas" && !tx.anulada);

    return matchesSearch && matchesMetodo && matchesEstado;
  });

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Historial de Transacciones</h1>
        </div>
      </div>

      {error && (
        <div className="error-banner" role="alert">
          <span>{error}</span>
        </div>
      )}

      {/* Filter and Search Bar Card */}
      <div className="filter-wrap-card">
        <div className="filter-left-col">
          {/* Search bar input */}
          <div className="select-group">
            <label className="select-grouplabel">Buscar venta</label>
            <div className="relative">
              <input
                type="text"
                placeholder="Cliente, ID u orden..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="caja-input"
                style={{ paddingLeft: "36px", minWidth: "260px" }}
              />
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">
                <Search size={16} />
              </span>
            </div>
          </div>

          {/* Payment Method filter */}
          <div className="select-group">
            <label className="select-grouplabel">Medio de Pago</label>
            <select
              value={filterMetodo}
              onChange={(e) => setFilterMetodo(e.target.value)}
              className="styled-select"
            >
              <option value="todos">Todos los medios</option>
              <option value="Efectivo">Efectivo</option>
              <option value="Tarjeta">Tarjeta de Crédito</option>
              <option value="Yape">Yape</option>
              <option value="Plin">Plin</option>
            </select>
          </div>

          {/* Status filter */}
          <div className="select-group">
            <label className="select-grouplabel">Estado</label>
            <select
              value={filterEstado}
              onChange={(e) => setFilterEstado(e.target.value)}
              className="styled-select"
            >
              <option value="todos">Todos los Estados</option>
              <option value="completadas">Completadas</option>
              <option value="anuladas">Anuladas</option>
            </select>
          </div>
        </div>

        <div className="total-counter">
          Mostrando <span className="total-num">{filteredTx.length}</span> transacciones
        </div>
      </div>

      {loading ? (
        <div className="loading-spinner-wrap">
          <div className="spinner"></div>
          <span>Cargando transacciones históricas...</span>
        </div>
      ) : filteredTx.length === 0 ? (
        <div className="empty-state">
          No se encontraron transacciones registradas que coincidan con los filtros actuales.
        </div>
      ) : (
        <div className="table-wrap">
          <table className="pos-table">
            <thead>
              <tr>
                <th>ID Venta</th>
                <th>Fecha y Hora</th>
                <th>Operador</th>
                <th>Cliente / Comprobante</th>
                <th>Método</th>
                <th>Estado</th>
                <th className="text-right">Total</th>
                <th style={{ width: "80px" }}>Acción</th>
              </tr>
            </thead>
            <tbody>
              {filteredTx.map((tx) => (
                <tr key={tx.transaccionId}>
                  <td className="font-mono text-sm font-semibold text-gray-500">
                    #{String(tx.transaccionId).padStart(4, "0")}
                  </td>
                  <td className="text-sm">
                    {new Date(tx.fecha).toLocaleString("es-PE", {
                      day: "2-digit",
                      month: "2-digit",
                      year: "numeric",
                      hour: "2-digit",
                      minute: "2-digit",
                    })}
                  </td>
                  <td className="font-medium text-sm">
                    {tx.operadorNombre || "Administrador"}
                  </td>
                  <td>
                    <div className="product-name-block">
                      <span className="product-display-name">{tx.clienteNombre || "Público General"}</span>
                      {tx.tipoDocumento && tx.numeroDocumento && (
                        <span className="product-display-description">
                          {tx.tipoDocumento} - {tx.numeroDocumento}
                        </span>
                      )}
                    </div>
                  </td>
                  <td>
                    <span className="inline-flex items-center gap-1.5 text-xs font-semibold px-2 py-1 bg-gray-100 rounded-md">
                      {tx.metodoPago}
                      {tx.metodoPagoSecundario && ` / ${tx.metodoPagoSecundario}`}
                    </span>
                  </td>
                  <td>
                    {tx.anulada ? (
                      <span className="px-2.5 py-1 bg-red-50 text-red-700 text-xs font-bold rounded-md inline-flex items-center gap-1">
                        <XCircle size={12} /> Anulada
                      </span>
                    ) : (
                      <span className="px-2.5 py-1 bg-emerald-50 text-emerald-700 text-xs font-bold rounded-md inline-flex items-center gap-1">
                        <CheckCircle size={12} /> Completada
                      </span>
                    )}
                  </td>
                  <td className="font-mono font-bold text-sm text-right">
                    S/. {tx.total.toFixed(2)}
                  </td>
                  <td>
                    <button
                      onClick={() => handleOpenDetail(tx.transaccionId)}
                      className="btn-action-edit"
                      title="Ver Detalle"
                    >
                      <Eye size={16} />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Detail Overlay Drawer/Modal */}
      {selectedTxId !== null && (
        <div className="modal-overlay" role="presentation" onClick={handleCloseDetail}>
          <div 
            className="modal-box max-w-lg" 
            role="dialog"
            aria-modal="true"
            aria-labelledby="tx-modal-title"
            ref={dialogRef}
            tabIndex={-1}
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h2 id="tx-modal-title">Detalle de Orden #{String(selectedTxId).padStart(4, "0")}</h2>
              <button onClick={handleCloseDetail} className="modal-close" aria-label="Cerrar modal">×</button>
            </div>

            {loadingDetail ? (
              <div className="loading-spinner-wrap" style={{ padding: "40px" }}>
                <div className="spinner"></div>
                <span>Obteniendo detalles del ticket...</span>
              </div>
            ) : errorDetail ? (
              <div className="error-banner">
                <span>{errorDetail}</span>
              </div>
            ) : txDetail ? (
              <div>
                {/* Visual design simulating ticket */}
                <div 
                  className="bg-amber-50/40 border border-amber-200/50 rounded-2xl p-6 font-mono text-xs text-slate-700 mb-6 shadow-inner"
                  style={{ backgroundImage: "linear-gradient(#fefbeb 1px, transparent 1px)", backgroundSize: "100% 20px" }}
                >
                  <div className="text-center border-b border-dashed border-amber-300 pb-4 mb-4">
                    <span className="font-bold text-sm tracking-wider uppercase block text-amber-900">Café de Barrio</span>
                    <span className="text-[10px] text-amber-800">CUSCO - CHINCHERO - PERÚ</span>
                    <p className="mt-1">Tlf: +51 984 123 456</p>
                  </div>

                  <div className="space-y-1 text-slate-600">
                    <p><strong>Fecha/Hora:</strong> {new Date(txDetail.fecha).toLocaleString("es-PE")}</p>
                    <p><strong>Cajero:</strong> {txDetail.operadorNombre || "Administrador"}</p>
                    <p><strong>Cliente:</strong> {txDetail.clienteNombre || "Público General"}</p>
                    {txDetail.numeroDocumento && (
                      <p><strong>{txDetail.tipoDocumento || "Comprobante"}:</strong> {txDetail.numeroDocumento}</p>
                    )}
                    {txDetail.razonSocial && (
                      <p><strong>Razón Social:</strong> {txDetail.razonSocial}</p>
                    )}
                  </div>

                  <div className="border-t border-b border-dashed border-amber-300 py-3 my-3">
                    <table className="w-full text-left file-table">
                      <thead>
                        <tr className="border-b border-amber-200 text-amber-800 font-bold">
                          <th className="pb-1" style={{ width: "55%" }}>Prod</th>
                          <th className="pb-1 text-center" style={{ width: "15%" }}>Cant</th>
                          <th className="pb-1 text-right" style={{ width: "30%" }}>Subtotal</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-dotted divide-amber-200">
                        {txDetail.items.map((it, idx) => (
                          <tr key={idx} className="text-slate-600">
                            <td className="py-2 pr-2">
                              <div>{it.nombreProducto}</div>
                              <span className="text-[10px] text-gray-400">S/. {it.precioUnitario.toFixed(2)} c/u</span>
                            </td>
                            <td className="py-2 text-center font-bold text-slate-800">{it.cantidad}</td>
                            <td className="py-2 text-right font-bold text-slate-800">S/. {it.subtotalLinea.toFixed(2)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  <div className="space-y-1 text-right font-bold">
                    <div className="flex justify-between text-slate-500 font-normal">
                      <span>Subtotal:</span>
                      <span>S/. {txDetail.subtotal.toFixed(2)}</span>
                    </div>
                    <div className="flex justify-between text-slate-500 font-normal">
                      <span>IGV (18%):</span>
                      <span>S/. {txDetail.igv.toFixed(2)}</span>
                    </div>
                    <div className="flex justify-between text-lg text-amber-900 border-t border-amber-200 pt-2 font-black mt-2 font-black uppercase tracking-tight">
                      <span>Costo Total:</span>
                      <span>S/. {txDetail.total.toFixed(2)}</span>
                    </div>
                  </div>

                  <div className="border-t border-dashed border-amber-300 pt-3 flex flex-col gap-1 text-[10px] text-slate-500 mt-4">
                    <p><strong>Medio de Pago Principal:</strong> {txDetail.metodoPago}</p>
                    {txDetail.metodoPagoSecundario && (
                      <>
                        <p><strong>Pago Secundario:</strong> {txDetail.metodoPagoSecundario}</p>
                        {txDetail.montoMetodoPrimario !== undefined && (
                          <p><strong>Efectivo Entregado:</strong> S/. {txDetail.montoMetodoPrimario.toFixed(2)}</p>
                        )}
                      </>
                    )}
                    <p><strong>Estado del Comprobante:</strong> {txDetail.anulada ? "ANULADA" : "ACTIVA/DESPACHADA"}</p>
                  </div>
                </div>

                <div className="flex justify-between gap-3">
                  <button 
                    onClick={() => alert("Impresión enviada a la ticketera térmica Epson TM-T20III")} 
                    className="btn btn-primary"
                    style={{ flex: 1 }}
                  >
                    <FileText size={16} /> Impresión Térmica
                  </button>
                  <button onClick={handleCloseDetail} className="btn btn-secondary" style={{ flex: 1 }}>
                    Cerrar Detalle
                  </button>
                </div>
              </div>
            ) : null}
          </div>
        </div>
      )}
    </div>
  );
}