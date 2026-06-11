import { useState, useEffect, startTransition, type FormEvent } from "react";
import { AlertCircle, Lock, Unlock, Check, ShieldCheck } from "lucide-react";
import { api } from "../api/client";
import { useAuth } from "../hooks/useAuth";
import { useNavigate } from "react-router-dom";
import type { TurnoActivoDto, OperadorDto, CerrarTurnoResultDto } from "../types";

export function ArqueoCaja() {
  const [turno, setTurno] = useState<TurnoActivoDto | null>(null);
  const [operadores, setOperadores] = useState<OperadorDto[]>([]);
  const [loadingCaja, setLoadingCaja] = useState<boolean>(true);
  const [errorCaja, setErrorCaja] = useState<string>("");
  const [successCaja, setSuccessCaja] = useState<string>("");

  const [selectedOp, setSelectedOp] = useState<number>(1);
  const [montoApertura, setMontoApertura] = useState<number>(150);

  const [montoCierto, setMontoCierto] = useState<number>(0);
  const [observaciones, setObservaciones] = useState<string>("");
  const [cierreResult, setCierreResult] = useState<CerrarTurnoResultDto | null>(null);
  const [processing, setProcessing] = useState<boolean>(false);

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
      const adminPrefix = "[Admin] ";
      const finalObs = observaciones.trim() ? `${adminPrefix}${observaciones.trim()}` : adminPrefix.trim();
      const res = await api.cerrarTurno(turno.turnoId, montoCierto, finalObs);
      setCierreResult(res);
      setSuccessCaja("Turno de POS cerrado correctamente. Revise los balances de arqueo.");
      setTurno(null); 
      setMontoCierto(0);
      setObservaciones("");
    } catch {
      setErrorCaja("No se pudo consolidar el cierre de caja.");
    } finally {
      setProcessing(false);
    }
  };

  return (
    <div>
      <div className="page-header mb-2">
        <div>
          <h1 className="page-title">Control de Arqueo de Caja</h1>
        </div>
      </div>

      <div className="section-header mb-6 mt-8">
        <h2 className="text-xl font-bold text-gray-900 flex items-center gap-2">
          <ShieldCheck size={22} className="text-amber-800" /> Control y Cierre
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
        <div className="loading-spinner-wrap loading-spinner-compact">
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
                            {op.nombre} (ID: {op.operadorId})
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
            <div className="card-title text-red-700">Cierre Forzado de Caja (Emergencia)</div>
            
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
                    placeholder="[Admin] Dejar nota técnica si hay sobrantes o faltantes..."
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
  );
}
