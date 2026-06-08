import { useState, useEffect, startTransition, type FormEvent } from "react";
import { ResponsiveContainer, LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Cell } from "recharts";
import { RefreshCw, AlertCircle, Lock, Unlock, Check, ShieldCheck } from "lucide-react";
import { api } from "../api/client";
import type { VentasResumenDto, VentasPorMetodoPagoDto, VentasPorDiaDto, TurnoActivoDto, OperadorDto, CerrarTurnoResultDto } from "../types";

export function ReportesYGraficos() {
  const [periodo, setPeriodo] = useState<string>("mes"); // 'dia' | 'semana' | 'mes'
  const [resumen, setResumen] = useState<VentasResumenDto | null>(null);
  const [ventasMetodo, setVentasMetodo] = useState<VentasPorMetodoPagoDto[]>([]);
  const [ventasDia, setVentasDia] = useState<VentasPorDiaDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>("");

  // Active/Closing Shift States
  const [turno, setTurno] = useState<TurnoActivoDto | null>(null);
  const [operadores, setOperadores] = useState<OperadorDto[]>([]);
  const [loadingCaja, setLoadingCaja] = useState<boolean>(true);
  const [errorCaja, setErrorCaja] = useState<string>("");
  const [successCaja, setSuccessCaja] = useState<string>("");

  // Apertura shift state
  const [selectedOp, setSelectedOp] = useState<number>(1);
  const [montoApertura, setMontoApertura] = useState<number>(150);

  // Cierre shift state
  const [montoCierto, setMontoCierto] = useState<number>(0);
  const [observaciones, setObservaciones] = useState<string>("");
  const [cierreResult, setCierreResult] = useState<CerrarTurnoResultDto | null>(null);
  const [processing, setProcessing] = useState<boolean>(false);

  const loadData = async () => {
    setLoading(true);
    setError("");
    try {
      const [resData, metData, diaData] = await Promise.all([
        api.ventasResumen(periodo),
        api.ventasPorMetodoPago(periodo),
        api.ventasPorDia(periodo),
      ]);
      setResumen(resData);
      setVentasMetodo(metData);
      setVentasDia(diaData);
    } catch {
      setError("No se pudo conectar con el servidor POS. Verifique su servicio.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    startTransition(() => {
      void loadData();
    });
  }, [periodo]);

  const loadCajaData = async () => {
    setLoadingCaja(true);
    setErrorCaja("");
    try {
      const [tActivo, ops] = await Promise.all([
        api.turnoActivo(),
        api.operadores()
      ]);
      setTurno(tActivo);
      const enabledOps = ops.filter(o => o.activo);
      setOperadores(enabledOps);
      if (enabledOps.length > 0) {
        setSelectedOp(enabledOps[0].operadorId);
      }
    } catch {
      setErrorCaja("No se pudo sincronizar el estado de turnos con el servidor.");
    } finally {
      setLoadingCaja(false);
    }
  };

  useEffect(() => {
    startTransition(() => {
      void loadCajaData();
    });
  }, []);

  const handleAbrirTurno = async (e: FormEvent) => {
    e.preventDefault();
    setProcessing(true);
    setErrorCaja("");
    setSuccessCaja("");
    try {
      await api.abrirTurno(selectedOp, montoApertura);
      setSuccessCaja("Turno de ventas abierto correctamente.");
      setCierreResult(null);
      void loadCajaData();
    } catch {
      setErrorCaja("No se pudo iniciar el nuevo turno de ventas.");
    } finally {
      setProcessing(false);
    }
  };

  const handleCerrarTurno = async (e: FormEvent) => {
    e.preventDefault();
    if (!turno) return;
    setProcessing(true);
    setErrorCaja("");
    setSuccessCaja("");
    try {
      const res = await api.cerrarTurno(turno.turnoId, montoCierto, observaciones);
      setCierreResult(res);
      setSuccessCaja("Turno de POS cerrado correctamente. Revise los balances de arqueo.");
      setTurno(null); // Turno is closed in memory
      setMontoCierto(0);
      setObservaciones("");
    } catch {
      setErrorCaja("No se pudo consolidar el cierre de caja.");
    } finally {
      setProcessing(false);
    }
  };

  const formatSales = (val: number) => {
    return `S/. ${val.toFixed(2)}`;
  };

  // Determine specific progress colors for methods
  const getMethodColorClass = (metodo: string) => {
    const m = metodo.toLowerCase();
    if (m.includes("efectivo")) return "bar-efectivo";
    if (m.includes("tarjeta")) return "bar-tarjeta";
    if (m.includes("yape")) return "bar-yape";
    return "bar-plin";
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Dashboard de Reportes</h1>
        </div>

        <div className="periodo-selector">
          <button
            onClick={() => setPeriodo("dia")}
            className={`periodo-btn ${periodo === "dia" ? "active" : ""}`}
          >
            Hoy
          </button>
          <button
            onClick={() => setPeriodo("semana")}
            className={`periodo-btn ${periodo === "semana" ? "active" : ""}`}
          >
            Semana
          </button>
          <button
            onClick={() => setPeriodo("mes")}
            className={`periodo-btn ${periodo === "mes" ? "active" : ""}`}
          >
            Este Mes
          </button>
        </div>
      </div>

      {error && (
        <div className="error-banner" role="alert">
          <AlertCircle size={18} />
          <span>{error}</span>
          <button onClick={() => { startTransition(() => { void loadData(); }); }} className="btn btn-secondary" style={{ padding: "4px 12px", marginLeft: "auto" }}>
            <RefreshCw size={12} /> Reintentar
          </button>
        </div>
      )}

      {loading ? (
        <div className="loading-spinner-wrap">
          <div className="spinner"></div>
          <span>Consolidando transacciones y reportes de caja...</span>
        </div>
      ) : (
        <>
          {/* KPI Dashboard Row */}
          {resumen && (
            <div className="kpi-grid">
              <div className="kpi-card">
                <span className="kpi-label">Ingresos Brutos</span>
                <span className="kpi-value">{formatSales(resumen.totalVentas)}</span>
                <span className="kpi-sub">Suma acumulada del período</span>
              </div>

              <div className="kpi-card">
                <span className="kpi-label">Transacciones</span>
                <span className="kpi-value" style={{ fontFamily: "var(--font-mono)" }}>
                  {resumen.numTransacciones}
                </span>
                <span className="kpi-sub">Tickets generados con éxito</span>
              </div>

              <div className="kpi-card">
                <span className="kpi-label">Efectivo en Caja</span>
                <span className="kpi-value">{formatSales(ventasMetodo.find(m => m.metodoPago.toLowerCase() === "efectivo")?.totalVentas || 0)}</span>
                <span className="kpi-sub">Fondo disponible para Arqueo</span>
              </div>
            </div>
          )}

          {/* Graph Layout Grid */}
          <div className="chart-row">
            {/* Sales Volume Curve */}
            <div className="card">
              <div className="card-title">Volumen y Curva de Ventas (S/.)</div>
              <div style={{ width: "100%", height: 280 }}>
                {ventasDia.length === 0 ? (
                  <div className="empty-state text-sm" style={{ height: "100%", contentVisibility: "auto" }}>
                    No hay suficientes datos de ventas para mostrar curvas temporales.
                  </div>
                ) : (
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={ventasDia} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E2E8F0" />
                      <XAxis
                        dataKey="fecha"
                        stroke="#64748B"
                        fontSize={11}
                        tickLine={false}
                        axisLine={false}
                        dy={10}
                      />
                      <YAxis
                        stroke="#64748B"
                        fontSize={11}
                        tickLine={false}
                        axisLine={false}
                        tickFormatter={(v) => `S/.${v}`}
                      />
                      <Tooltip
                        contentStyle={{
                          background: "#ffffff",
                          border: "1px solid #E2E8F0",
                          borderRadius: "12px",
                          fontFamily: "var(--font-sans)",
                          fontSize: "12px",
                          boxShadow: "0 4px 12px rgba(0,0,0,0.03)",
                        }}
                        formatter={(val: any) => [formatSales(val as number), "Ventas"]}
                      />
                      <Line
                        type="monotone"
                        dataKey="totalVentas"
                        stroke="#7C2D12"
                        strokeWidth={3}
                        dot={{ r: 4, strokeWidth: 2, fill: "#ffffff" }}
                        activeDot={{ r: 6 }}
                      />
                    </LineChart>
                  </ResponsiveContainer>
                )}
              </div>
            </div>

            {/* Categorías de Pago BarChart */}
            <div className="card">
              <div className="card-title">Participación por Método de Pago</div>
              <div style={{ width: "100%", height: 280 }}>
                {ventasMetodo.length === 0 ? (
                  <div className="empty-state text-sm" style={{ height: "100%" }}>
                    Ventas vacías para el período.
                  </div>
                ) : (
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={ventasMetodo} margin={{ top: 10, right: 5, left: -20, bottom: 0 }}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E2E8F0" />
                      <XAxis
                        dataKey="metodoPago"
                        stroke="#64748B"
                        fontSize={11}
                        axisLine={false}
                        tickLine={false}
                        dy={8}
                      />
                      <YAxis
                        stroke="#64748B"
                        fontSize={11}
                        axisLine={false}
                        tickLine={false}
                        tickFormatter={(v) => `S/.${v}`}
                      />
                      <Tooltip
                        contentStyle={{
                          background: "#ffffff",
                          border: "1px solid #E2E8F0",
                          borderRadius: "12px",
                          fontFamily: "var(--font-sans)",
                          fontSize: "12px",
                        }}
                        formatter={(val: any) => [formatSales(val as number), "Monto"]}
                      />
                      <Bar dataKey="totalVentas" radius={[6, 6, 0, 0]} maxBarSize={45}>
                        {ventasMetodo.map((entry, index) => {
                          const m = entry.metodoPago.toLowerCase();
                          let fill = "#7C2D12"; // Efectivo (artisanal rust red)
                          if (m.includes("tarjeta")) fill = "#D97706"; // Amber
                          if (m.includes("yape")) fill = "#92400E"; // warm dark amber
                          if (m.includes("plin")) fill = "#CBD5E1"; // light slate gray for Plin
                          return <Cell key={`cell-${index}`} fill={fill} />;
                        })}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                )}
              </div>
            </div>
          </div>

          {/* Desglose de Caja horizontal (Image 1 replica style) */}
          <div className="desglose-section select-none">
            <h2 className="desglose-title">Saldos Desglosados en Bóveda POS</h2>

            <div className="desglose-grid">
              {ventasMetodo.map((m) => {
                const totalMontoPeriodo = resumen?.totalVentas || 1;
                const percentage = Math.round((m.totalVentas / totalMontoPeriodo) * 100) || 0;
                
                return (
                  <div key={m.metodoPago} className="desglose-card">
                    <div className="desglose-header">
                      <span className="desglose-label">{m.metodoPago}</span>
                      <span className="desglose-percentage">{percentage}%</span>
                    </div>

                    <div className="desglose-progress-bg">
                      <div
                        className={`desglose-progress-bar ${getMethodColorClass(m.metodoPago)}`}
                        style={{ width: `${percentage}%` }}
                      ></div>
                    </div>

                    <div className="desglose-meta">
                      <span className="desglose-amount">S/. {m.totalVentas.toFixed(2)}</span>
                      <span className="desglose-ops">{m.numTransacciones} Ops</span>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="border-t border-gray-100 my-8 pt-8">
            <div className="section-header mb-6">
              <h2 className="text-xl font-bold text-gray-900 flex items-center gap-2">
                <ShieldCheck size={22} className="text-amber-800" /> Control de Arqueo y Cierre de Caja
              </h2>
              <p className="text-xs text-slate-500 mt-1">Supervisión en tiempo real de turnos y balances de los operadores</p>
            </div>

            {successCaja && (
              <div className="success-banner mb-6" role="status">
                <Check size={18} />
                <span>{successCaja}</span>
              </div>
            )}

            {errorCaja && (
              <div className="error-banner mb-6" role="alert">
                <AlertCircle size={18} />
                <span>{errorCaja}</span>
              </div>
            )}

            {loadingCaja ? (
              <div className="loading-spinner-wrap" style={{ padding: "20px 0" }}>
                <div className="spinner"></div>
                <span>Verificando estatus de caja registradora...</span>
              </div>
            ) : (
              <div className="caja-grid">
                {/* Active Shift Card or Opening Control Card */}
                <div className="card">
                  <div className="card-title">Estatus del Cajón POS</div>
                  
                  {turno ? (
                    <div>
                      <div className="caja-status abierto">
                        <Unlock size={20} />
                        <div>
                          <div className="caja-status-label">TURNO ACTIVO EN SISTEMA</div>
                          <span className="font-semibold text-sm text-amber-950">Abierto por {turno.nombreOperador}</span>
                        </div>
                      </div>

                      <div className="form-row border-top pt-6 mt-4">
                        <div>
                          <span className="text-muted text-xs font-semibold block">Monto de Apertura</span>
                          <span className="font-mono text-sm font-bold">S/. {turno.montoApertura.toFixed(2)}</span>
                        </div>
                        <div>
                          <span className="text-muted text-xs font-semibold block">Fecha de Apertura</span>
                          <span className="text-sm font-medium">{new Date(turno.fechaApertura).toLocaleTimeString("es-PE")}</span>
                        </div>
                      </div>
                    </div>
                  ) : (
                    <div>
                      <div className="caja-status cerrado">
                        <Lock size={20} />
                        <div>
                          <div className="caja-status-label">CASH SHIFT CERRADO</div>
                          <span className="font-semibold text-sm">Abra un nuevo turno para operar</span>
                        </div>
                      </div>

                      {operadores.length === 0 ? (
                        <div className="empty-state text-sm">
                          No hay operadores con estado Habilitado registrados para abrir la caja.
                        </div>
                      ) : (
                        <form onSubmit={handleAbrirTurno} className="caja-form mt-4">
                          <label className="caja-form-label" htmlFor="apertura-cajero">
                            Seleccionar Cajero Responsable
                            <select
                              id="apertura-cajero"
                              className="caja-input"
                              value={selectedOp}
                              onChange={(e) => setSelectedOp(Number(e.target.value))}
                            >
                              {operadores.map(op => (
                                <option key={op.operadorId} value={op.operadorId}>
                                  {op.nombre}
                                </option>
                              ))}
                            </select>
                          </label>

                          <label className="caja-form-label" htmlFor="apertura-monto">
                            Monto Inicial de Caja (S/.)
                            <input
                              id="apertura-monto"
                              type="number"
                              min="0"
                              className="caja-input caja-input-mono text-sm"
                              value={montoApertura}
                              onChange={(e) => setMontoApertura(Number(e.target.value))}
                              required
                            />
                          </label>

                          <button type="submit" className="btn btn-primary mt-20" disabled={processing}>
                            {processing ? "Abriendo..." : "Iniciar Turno en POS"}
                          </button>
                        </form>
                      )}
                    </div>
                  )}
                </div>

                {/* Action / Report Card */}
                <div className="card">
                  <div className="card-title">Acción de Arqueo y Cierre</div>
                  
                  {turno ? (
                    <form onSubmit={handleCerrarTurno} className="caja-form">
                      <label className="caja-form-label" htmlFor="cierre-cierto">
                        Monto Físico Arqueado en Efectivo (S/.)
                        <input
                          id="cierre-cierto"
                          type="number"
                          step="0.01"
                          min="0"
                          placeholder="S/. 0.00"
                          className="caja-input caja-input-mono"
                          value={montoCierto || ""}
                          onChange={(e) => setMontoCierto(Number(e.target.value))}
                          required
                        />
                        <span className="text-muted text-xs mt-4">
                          Ingrese el conteo billete por billete físico retirado del cajón de efectivo.
                        </span>
                      </label>

                      <label className="caja-form-label" htmlFor="cierre-obs">
                        Observaciones / Variaciones detectadas
                        <textarea
                          id="cierre-obs"
                          className="caja-input"
                          rows={3}
                          placeholder="Dejar nota técnica si hay sobrantes o faltantes..."
                          value={observaciones}
                          onChange={(e) => setObservaciones(e.target.value)}
                        />
                      </label>

                      <button type="submit" className="btn btn-danger mt-4" disabled={processing}>
                        {processing ? "Cerrando..." : "Realizar Cierre de Caja"}
                      </button>
                    </form>
                  ) : (
                    <div>
                      {cierreResult ? (
                        <div className="result-caja">
                          <span className="text-muted text-xs font-bold uppercase block mb-8">
                            Resultado del Último Cierre
                          </span>
                          
                          <div className="result-row mt-4">
                            <span>Efectivo registrado en Sistema:</span>
                            <span className="font-mono font-bold">S/. {cierreResult.totalEfectivoSistema.toFixed(2)}</span>
                          </div>

                          <div className="result-row border-top pt-6">
                            <span>Efectivo arqueado por cajero:</span>
                            <span className="font-mono font-bold">S/. {cierreResult.montoEfectivoCierto.toFixed(2)}</span>
                          </div>

                          <div className="result-row border-top pt-6 font-bold">
                            <span>Discrepancia / Diferencia de Caja:</span>
                            <span
                              className={cierreResult.diferencia >= 0 ? "dif-pos font-semibold" : "dif-neg font-semibold"}
                            >
                              S/. {cierreResult.diferencia.toFixed(2)}
                            </span>
                          </div>

                          <p className="text-muted text-xs mt-20 italic">
                            {cierreResult.diferencia === 0
                              ? "Cuadre perfecto. No se registran inconsistencias."
                              : cierreResult.diferencia > 0
                              ? "Sobrante detectado en el cajón."
                              : "Faltante detectado. Audite las transacciones del turno."}
                          </p>
                        </div>
                      ) : (
                        <div className="empty-state text-sm">
                          No se han consolidado arqueos de caja en la sesión actual.
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}