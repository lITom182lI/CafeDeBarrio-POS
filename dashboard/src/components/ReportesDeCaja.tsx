import { useState, useEffect, startTransition, useRef } from "react";
import { Search, Info, FileText } from "lucide-react";
import { api } from "../api/client";
import type { TurnoCerradoDto } from "../types";

export function ReportesDeCaja() {
  const [turnos, setTurnos] = useState<TurnoCerradoDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>("");
  
  // Search / Filters State
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [filterEstado, setFilterEstado] = useState<string>("todos");

  // Detail Modal State
  const [selectedTurnoId, setSelectedTurnoId] = useState<number | null>(null);
  
  // Observaciones Modal State
  const [obsTurno, setObsTurno] = useState<TurnoCerradoDto | null>(null);

  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (selectedTurnoId !== null || obsTurno !== null) dialogRef.current?.focus();
  }, [selectedTurnoId, obsTurno]);

  const loadTurnos = async () => {
    setLoading(true);
    setError("");
    try {
      const data = await api.turnosCerrados("mes");
      setTurnos(data);
    } catch {
      setError("No se pudo obtener el historial de cierres de caja. Reintentando...");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    startTransition(() => {
      void loadTurnos();
    });
  }, []);

  const handleOpenDetail = (id: number) => {
    setSelectedTurnoId(id);
  };

  const handleCloseDetail = () => {
    setSelectedTurnoId(null);
  };

  const handleOpenObs = (turno: TurnoCerradoDto) => {
    setObsTurno(turno);
  };

  const handleCloseObs = () => {
    setObsTurno(null);
  };

  // Filter & Search Logic
  const filteredTurnos = turnos.filter((t) => {
    const searchLower = searchTerm.trim().toLowerCase();
    const matchesSearch = searchLower === "" ||
      (t.operadorNombre?.toLowerCase() ?? "").includes(searchLower) ||
      String(t.turnoId).includes(searchLower);

    const matchesEstado =
      filterEstado === "todos" ||
      t.estadoCierre.toLowerCase() === filterEstado;

    return matchesSearch && matchesEstado;
  });

  const selectedTurno = turnos.find(t => t.turnoId === selectedTurnoId);

  const getEstadoBadge = (estado: string) => {
    if (estado === "Sobrante") {
      return <span className="inline-flex items-center gap-1 text-[11px] font-bold text-sky-700 bg-sky-50 px-2 py-0.5 rounded-full border border-sky-200">Sobrante</span>;
    }
    if (estado === "Faltante") {
      return <span className="inline-flex items-center gap-1 text-[11px] font-bold text-rose-700 bg-rose-50 px-2 py-0.5 rounded-full border border-rose-200">Faltante</span>;
    }
    return <span className="inline-flex items-center gap-1 text-[11px] font-bold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-full border border-emerald-200">Cuadrado</span>;
  }

  const formatSoles = (v: number) => `S/. ${v.toFixed(2)}`;

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Historial de Arqueos y Cierres de Caja</h1>
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
            <label className="select-grouplabel">Buscar arqueo</label>
            <div className="relative">
              <input
                type="text"
                placeholder="ID Cajero o Nombre..."
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

          {/* Status filter */}
          <div className="select-group">
            <label className="select-grouplabel">Estado</label>
            <select
              value={filterEstado}
              onChange={(e) => setFilterEstado(e.target.value)}
              className="styled-select"
            >
              <option value="todos">Todos los Estados</option>
              <option value="cuadrado">Cuadrado</option>
              <option value="sobrante">Sobrante</option>
              <option value="faltante">Faltante</option>
            </select>
          </div>
        </div>

        <div className="total-counter">
          Mostrando <span className="total-num">{filteredTurnos.length}</span> arqueos
        </div>
      </div>

      {loading ? (
        <div className="loading-spinner-wrap">
          <div className="spinner"></div>
          <span>Cargando históricos de caja...</span>
        </div>
      ) : filteredTurnos.length === 0 ? (
        <div className="empty-state">
          No se encontraron arqueos registrados que coincidan con los filtros actuales.
        </div>
      ) : (
        <div className="table-wrap">
          <table className="pos-table">
            <thead>
              <tr>
                <th>ID Turno</th>
                <th>Operador</th>
                <th>Apertura</th>
                <th>Cierre</th>
                <th className="text-right">Monto Inicial</th>
                <th className="text-right">Monto Arqueado</th>
                <th className="text-center">Estado</th>
                <th style={{ width: "100px" }}>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {filteredTurnos.map((t) => (
                <tr key={t.turnoId}>
                  <td className="font-mono text-sm font-semibold text-gray-500">
                    #{String(t.turnoId).padStart(4, "0")}
                  </td>
                  <td className="font-medium text-slate-800 flex items-center gap-1">
                    {t.observaciones?.startsWith('[Admin]') ? 'Admin' : t.operadorNombre} 
                    {!t.observaciones?.startsWith('[Admin]') && <span className="text-gray-400 text-xs">(ID: {t.operadorId})</span>}
                  </td>
                  <td className="text-sm">
                    {new Date(t.fechaApertura).toLocaleString("es-PE", {
                      day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit",
                    })}
                  </td>
                  <td className="text-sm">
                    {new Date(t.fechaCierre).toLocaleString("es-PE", {
                      day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit",
                    })}
                  </td>
                  <td className="text-right font-semibold text-gray-700">
                    {formatSoles(t.montoApertura)}
                  </td>
                  <td className="text-right font-black text-amber-900">
                    {formatSoles(t.montoEfectivoCierto)}
                  </td>
                  <td className="text-center">
                    {getEstadoBadge(t.estadoCierre)}
                  </td>
                  <td className="text-center">
                    <div className="flex gap-2 justify-center">
                      <button onClick={() => handleOpenDetail(t.turnoId)} className="btn-action-edit" title="Ver resultado de cierre">
                        <FileText size={16} />
                      </button>
                      <button onClick={() => handleOpenObs(t)} className="btn-action-edit text-amber-600" title="Ver observaciones técnicas">
                        <Info size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Ticket/Resultado de Cierre Modal */}
      {selectedTurno && (
        <div className="modal-overlay" role="presentation" onClick={handleCloseDetail}>
          <div 
            className="modal-box max-w-sm" 
            role="dialog"
            aria-modal="true"
            aria-labelledby="tx-modal-title"
            ref={dialogRef}
            tabIndex={-1}
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h2 id="tx-modal-title">Ticket de Arqueo #{String(selectedTurno.turnoId).padStart(4, "0")}</h2>
              <button onClick={handleCloseDetail} className="modal-close" aria-label="Cerrar modal">×</button>
            </div>

            <div>
              {/* Visual design simulating ticket */}
              <div 
                className="bg-slate-50 border border-slate-200 rounded-xl p-6 font-mono text-xs text-slate-700 mb-6 shadow-inner"
              >
                <div className="text-center border-b border-dashed border-slate-300 pb-4 mb-4">
                  <h3 className="font-black text-sm tracking-widest uppercase block text-slate-900 mb-1">Resultado de Cierre</h3>
                  <span className="text-[10px] text-slate-500 font-medium block mb-1">Cajera/o: {selectedTurno.operadorNombre}</span>
                </div>

                <div className="space-y-1 text-slate-600 text-[11px]">
                  <p><strong>Apertura:</strong> {new Date(selectedTurno.fechaApertura).toLocaleString("es-PE")}</p>
                  <p><strong>Cierre:</strong> {new Date(selectedTurno.fechaCierre).toLocaleString("es-PE")}</p>
                </div>

                <div className="border-t border-b border-dashed border-slate-300 py-3 my-3">
                  <div className="space-y-2 text-right">
                    <div className="flex justify-between font-normal">
                      <span>Monto Inicial en Caja:</span>
                      <span>{formatSoles(selectedTurno.montoApertura)}</span>
                    </div>
                    <div className="flex justify-between font-normal text-sky-700">
                      <span>Efectivo Total Sistema:</span>
                      <span>{formatSoles(selectedTurno.totalEfectivoSistema)}</span>
                    </div>
                    <div className="flex justify-between font-bold text-slate-800">
                      <span>Monto Arqueado Físico:</span>
                      <span>{formatSoles(selectedTurno.montoEfectivoCierto)}</span>
                    </div>
                  </div>
                </div>

                <div className="space-y-1 text-right font-bold text-sm">
                  <div className={`flex justify-between ${selectedTurno.diferencia < 0 ? 'text-rose-600' : selectedTurno.diferencia > 0 ? 'text-sky-600' : 'text-emerald-600'}`}>
                    <span>Diferencia ({selectedTurno.estadoCierre}):</span>
                    <span>{formatSoles(selectedTurno.diferencia)}</span>
                  </div>
                </div>
              </div>

              <div className="flex justify-center">
                <button onClick={handleCloseDetail} className="btn btn-secondary w-full">
                  Cerrar
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Observaciones Modal */}
      {obsTurno && (
        <div className="modal-overlay" role="presentation" onClick={handleCloseObs}>
          <div 
            className="modal-box max-w-md" 
            role="dialog"
            aria-modal="true"
            aria-labelledby="obs-modal-title"
            tabIndex={-1}
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h2 id="obs-modal-title">Observaciones Técnicas</h2>
              <button onClick={handleCloseObs} className="modal-close" aria-label="Cerrar modal">×</button>
            </div>

            <div className="p-4 bg-amber-50 text-amber-900 border border-amber-200 rounded-lg text-sm mb-4">
              {obsTurno.observaciones ? (
                <p className="whitespace-pre-wrap">{obsTurno.observaciones}</p>
              ) : (
                <p className="text-amber-700/60 italic">No se registraron observaciones durante el cierre de caja.</p>
              )}
            </div>

            <div className="flex justify-end">
              <button onClick={handleCloseObs} className="btn btn-primary">
                Aceptar
              </button>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}
