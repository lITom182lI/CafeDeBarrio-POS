import { useState, useEffect, startTransition, useCallback } from "react";
import { ResponsiveContainer, LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Cell } from "recharts";
import { RefreshCw, AlertCircle } from "lucide-react";
import { api } from "../api/client";
import type { VentasResumenDto, VentasPorMetodoPagoDto, VentasPorDiaDto, AnulacionResumenDto } from "../types";

export function ReportesYGraficos() {
  const [periodo, setPeriodo] = useState<string>("mes"); // 'dia' | 'semana' | 'mes'
  const [resumen, setResumen] = useState<VentasResumenDto | null>(null);
  const [ventasMetodo, setVentasMetodo] = useState<VentasPorMetodoPagoDto[]>([]);
  const [ventasDia, setVentasDia] = useState<VentasPorDiaDto[]>([]);
  const [anulacionesData, setAnulacionesData] = useState<AnulacionResumenDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>("");



  const loadData = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const [resData, metData, diaData, anuData] = await Promise.all([
        api.ventasResumen(periodo),
        api.ventasPorMetodoPago(periodo),
        api.ventasPorDia(periodo),
        api.anulaciones(periodo)
      ]);
      setResumen(resData);
      setVentasMetodo(metData);
      setVentasDia(diaData);
      setAnulacionesData(anuData);
    } catch {
      setError("No se pudo conectar con el servidor POS. Verifique su servicio.");
    } finally {
      setLoading(false);
    }
  }, [periodo]);

  useEffect(() => {
    startTransition(() => {
      void loadData();
    });
  }, [loadData]);



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
          <button onClick={() => { startTransition(() => { void loadData(); }); }} className="btn btn-secondary error-banner-action">
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
                <span className="kpi-value kpi-mono">
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
              <div className="chart-container-wrapper">
                {ventasDia.length === 0 ? (
                  <div className="empty-state text-sm empty-state-chart">
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
                        formatter={(val: number | string | readonly (number | string)[] | undefined) => [formatSales(Number(val ?? 0)), "Ventas"]}
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
              <div className="chart-container-wrapper">
                {ventasMetodo.length === 0 ? (
                  <div className="empty-state text-sm empty-state-chart">
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
                        formatter={(val: number | string | readonly (number | string)[] | undefined) => [formatSales(Number(val ?? 0)), "Monto"]}
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

            {/* Cantidad de Ventas por Estado BarChart */}
            <div className="card">
              <div className="card-title">Cantidad de Ventas por Estado</div>
              <div className="chart-container-wrapper">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={[
                    { name: "Completadas", count: resumen?.numTransacciones || 0 },
                    { name: "Anuladas", count: anulacionesData.length }
                  ]} margin={{ top: 10, right: 5, left: -20, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E2E8F0" />
                    <XAxis dataKey="name" stroke="#64748B" fontSize={11} axisLine={false} tickLine={false} dy={8} />
                    <YAxis stroke="#64748B" fontSize={11} axisLine={false} tickLine={false} />
                    <Tooltip contentStyle={{ borderRadius: "12px" }} formatter={(val: number | string | readonly (number | string)[] | undefined) => [val ?? 0, "Cantidad"]} />
                    <Bar dataKey="count" radius={[6, 6, 0, 0]} maxBarSize={45}>
                      <Cell fill="#10B981" />
                      <Cell fill="#EF4444" />
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* Impacto Económico por Estado BarChart */}
            <div className="card">
              <div className="card-title">Impacto Económico por Estado</div>
              <div className="chart-container-wrapper">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={[
                    { name: "Completadas", monto: resumen?.totalVentas || 0 },
                    { name: "Anuladas", monto: anulacionesData.reduce((acc, curr) => acc + curr.montoDevuelto, 0) }
                  ]} margin={{ top: 10, right: 5, left: -20, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E2E8F0" />
                    <XAxis dataKey="name" stroke="#64748B" fontSize={11} axisLine={false} tickLine={false} dy={8} />
                    <YAxis stroke="#64748B" fontSize={11} axisLine={false} tickLine={false} tickFormatter={(v) => `S/.${v}`} />
                    <Tooltip contentStyle={{ borderRadius: "12px" }} formatter={(val: number | string | readonly (number | string)[] | undefined) => [formatSales(Number(val ?? 0)), "Monto"]} />
                    <Bar dataKey="monto" radius={[6, 6, 0, 0]} maxBarSize={45}>
                      <Cell fill="#10B981" />
                      <Cell fill="#EF4444" />
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
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
                        className={`desglose-progress-bar ${getMethodColorClass(m.metodoPago)} w-pct-${percentage}`}
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


        </>
      )}
    </div>
  );
}