import { useState, useEffect, startTransition } from "react";
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

  const loadTransactions = async () => {
    setLoading(true);
    setError("");
    try {
      const data = await api.transacciones();
      setTransacciones(data);
    } catch {
      setError("No se pudo obtener el historial de transacciones.");
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
    const term = searchTerm.toLowerCase();
    const matchesSearch =
      !searchTerm ||
      (tx.clienteNombre?.toLowerCase().includes(term)) ||
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
        <h1 className="page-title">Historial de Transacciones</h1>
      </div>

      {error && (
        <div className="error-banner" role="alert">
          <span>{error}</span>
        </div>
      )}

      {/* Filter and Search Bar Card */}
      <div className="filter-wrap-card">
        <div className="filter-left-col">
          {/* Search bar */}
          <div className="select-group">
            <label className="select-grouplabel">Buscar venta</label>
            <div style={{ position: "relative", display: "flex", alignItems: "center" }}>
              <span style={{ position: "absolute", left: "12px", color: "#64748B", display: "flex", alignItems: "center", pointerEvents: "none" }}>
                <Search size={16} />
              </span>
              <input
                type="text"
                placeholder="Cliente, ID u orden..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="caja-input"
                style={{ paddingLeft: "36px", minWidth: "240px" }}
              />
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
          No se encontraron transacciones que coincidan con los filtros actuales.
        </div>
      ) : (
        <div className="table-wrap">
          <table className="pos-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Fecha y Hora</th>
                <th>Operador</th>
                <th>Cliente / Comprobante</th>
                <th>Método</th>
                <th>Estado</th>
                <th style={{ textAlign: "right" }}>Total</th>
                <th style={{ width: "70px" }}>Acción</th>
              </tr>
            </thead>
            <tbody>
              {filteredTx.map((tx) => (
                <tr key={tx.transaccionId}>
                  <td>
                    <span className="mono-price fw-semibold" style={{ color: "#64748B" }}>
                      #{String(tx.transaccionId).padStart(4, "0")}
                    </span>
                  </td>
                  <td style={{ fontSize: "0.8125rem" }}>
                    {new Date(tx.fecha).toLocaleString("es-PE", {
                      day: "2-digit",
                      month: "2-digit",
                      year: "numeric",
                      hour: "2-digit",
                      minute: "2-digit",
                    })}
                  </td>
                  <td style={{ fontSize: "0.875rem", fontWeight: 500 }}>
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
                    <span style={{
                      display: "inline-flex",
                      alignItems: "center",
                      gap: "6px",
                      fontSize: "0.75rem",
                      fontWeight: 600,
                      padding: "3px 10px",
                      background: "#F1F5F9",
                      borderRadius: "8px",
                      color: "#334155"
                    }}>
                      {tx.metodoPago}
                      {tx.metodoPagoSecundario && ` / ${tx.metodoPagoSecundario}`}
                    </span>
                  </td>
                  <td>
                    {tx.anulada ? (
                      <span style={{
                        display: "inline-flex", alignItems: "center", gap: "4px",
                        padding: "3px 10px", background: "#FEF2F2",
                        color: "#DC2626", fontSize: "0.75rem", fontWeight: 700,
                        borderRadius: "8px"
                      }}>
                        <XCircle size={12} /> Anulada
                      </span>
                    ) : (
                      <span style={{
                        display: "inline-flex", alignItems: "center", gap: "4px",
                        padding: "3px 10px", background: "#ECFDF5",
                        color: "#059669", fontSize: "0.75rem", fontWeight: 700,
                        borderRadius: "8px"
                      }}>
                        <CheckCircle size={12} /> Completada
                      </span>
                    )}
                  </td>
                  <td style={{ textAlign: "right" }}>
                    <span className="mono-price fw-bold">S/. {tx.total.toFixed(2)}</span>
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

      {/* Detail Modal */}
      {selectedTxId !== null && (
        <div className="modal-overlay" onClick={handleCloseDetail}>
          <div className="modal-box" style={{ maxWidth: "520px" }} onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Detalle de Orden #{String(selectedTxId).padStart(4, "0")}</h2>
              <button onClick={handleCloseDetail} className="modal-close">×</button>
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
                {/* Ticket visual */}
                <div style={{
                  background: "linear-gradient(#fefbeb 1px, transparent 1px)",
                  backgroundSize: "100% 20px",
                  backgroundColor: "rgba(251, 243, 219, 0.4)",
                  border: "1px solid rgba(217, 182, 115, 0.5)",
                  borderRadius: "16px",
                  padding: "24px",
                  fontFamily: "var(--font-mono)",
                  fontSize: "0.75rem",
                  color: "#475569",
                  marginBottom: "20px"
                }}>
                  <div style={{ textAlign: "center", borderBottom: "1px dashed #D97706", paddingBottom: "12px", marginBottom: "12px" }}>
                    <span style={{ fontWeight: 700, fontSize: "0.875rem", letterSpacing: "0.05em", textTransform: "uppercase", display: "block", color: "#78350F" }}>
                      Café de Barrio
                    </span>
                    <span style={{ fontSize: "0.625rem", color: "#92400E" }}>CUSCO - CHINCHERO - PERÚ</span>
                    <p style={{ marginTop: "4px" }}>Tlf: +51 984 123 456</p>
                  </div>

                  <div style={{ display: "flex", flexDirection: "column", gap: "4px", color: "#475569" }}>
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

                  <div style={{ borderTop: "1px dashed #D97706", borderBottom: "1px dashed #D97706", padding: "12px 0", margin: "12px 0" }}>
                    <table style={{ width: "100%", borderCollapse: "collapse", textAlign: "left" }}>
                      <thead>
                        <tr style={{ borderBottom: "1px solid #D97706", color: "#92400E", fontWeight: 700 }}>
                          <th style={{ paddingBottom: "6px", width: "55%" }}>Prod</th>
                          <th style={{ paddingBottom: "6px", textAlign: "center", width: "15%" }}>Cant</th>
                          <th style={{ paddingBottom: "6px", textAlign: "right", width: "30%" }}>Subtotal</th>
                        </tr>
                      </thead>
                      <tbody>
                        {txDetail.items.map((it, idx) => (
                          <tr key={idx} style={{ color: "#475569", borderBottom: "1px dotted #D97706" }}>
                            <td style={{ padding: "8px 8px 8px 0" }}>
                              <div>{it.nombreProducto}</div>
                              <span style={{ fontSize: "0.625rem", color: "#94A3B8" }}>S/. {it.precioUnitario.toFixed(2)} c/u</span>
                            </td>
                            <td style={{ padding: "8px", textAlign: "center", fontWeight: 700, color: "#1E293B" }}>{it.cantidad}</td>
                            <td style={{ padding: "8px 0 8px 8px", textAlign: "right", fontWeight: 700, color: "#1E293B" }}>S/. {it.subtotalLinea.toFixed(2)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  <div style={{ display: "flex", flexDirection: "column", gap: "4px", textAlign: "right" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", color: "#64748B", fontWeight: 400 }}>
                      <span>Subtotal:</span>
                      <span>S/. {txDetail.subtotal.toFixed(2)}</span>
                    </div>
                    <div style={{ display: "flex", justifyContent: "space-between", color: "#64748B", fontWeight: 400 }}>
                      <span>IGV (18%):</span>
                      <span>S/. {txDetail.igv.toFixed(2)}</span>
                    </div>
                    <div style={{
                      display: "flex", justifyContent: "space-between",
                      fontSize: "1.125rem", color: "#78350F",
                      borderTop: "1px solid #D97706", paddingTop: "8px",
                      fontWeight: 900, textTransform: "uppercase", letterSpacing: "-0.01em", marginTop: "8px"
                    }}>
                      <span>Costo Total:</span>
                      <span>S/. {txDetail.total.toFixed(2)}</span>
                    </div>
                  </div>

                  <div style={{ borderTop: "1px dashed #D97706", paddingTop: "12px", marginTop: "16px", display: "flex", flexDirection: "column", gap: "4px", fontSize: "0.625rem", color: "#64748B" }}>
                    <p><strong>Medio de Pago Principal:</strong> {txDetail.metodoPago}</p>
                    {txDetail.metodoPagoSecundario && (
                      <>
                        <p><strong>Pago Secundario:</strong> {txDetail.metodoPagoSecundario}</p>
                        {txDetail.montoMetodoPrimario !== undefined && (
                          <p><strong>Efectivo Entregado:</strong> S/. {txDetail.montoMetodoPrimario.toFixed(2)}</p>
                        )}
                      </>
                    )}
                    <p><strong>Estado:</strong> {txDetail.anulada ? "ANULADA" : "ACTIVA/DESPACHADA"}</p>
                  </div>
                </div>

                <div style={{ display: "flex", gap: "12px" }}>
                  <button
                    onClick={() => alert("Impresión enviada a la ticketera térmica")}
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