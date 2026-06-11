import { useState, useEffect, startTransition, type FormEvent, useRef } from "react";
import { Plus, Check, AlertCircle } from "lucide-react";
import { api } from "../api/client";
import type { OperadorDto } from "../types";

export function GestionOperadores() {
  const [operadores, setOperadores] = useState<OperadorDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>("");
  const [success, setSuccess] = useState<string>("");

  // Modal State
  const [modalOpen, setModalOpen] = useState<boolean>(false);
  const [selectedOperador, setSelectedOperador] = useState<OperadorDto | null>(null);

  // Form State
  const [formNombre, setFormNombre] = useState<string>("");
  const [formPin, setFormPin] = useState<string>("");
  const [formActivo, setFormActivo] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [formError, setFormError] = useState<string>("");
  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (modalOpen) dialogRef.current?.focus();
  }, [modalOpen]);

  const loadData = async () => {
    setLoading(true);
    setError("");
    try {
      const ops = await api.operadores();
      setOperadores(ops);
    } catch {
      setError("No se pudo conectar con la API para recopilar la lista de operadores.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    startTransition(() => {
      void loadData();
    });
  }, []);

  const handleOpenNewModal = () => {
    setSelectedOperador(null);
    setFormNombre("");
    setFormPin("");
    setFormActivo(true);
    setFormError("");
    setModalOpen(true);
  };

  const handleOpenEditModal = (op: OperadorDto) => {
    setSelectedOperador(op);
    setFormNombre(op.nombre);
    setFormPin(""); // Pin is hidden in inputs
    setFormActivo(op.activo);
    setFormError("");
    setModalOpen(true);
  };

  const handleDeleteOperador = async (id: number, name: string) => {
    if (!confirm(`¿Está seguro de que desea eliminar el operador "${name}" del sistema POS?`)) {
      return;
    }
    setError("");
    try {
      await api.eliminarOperador(id);
      setSuccess(`Operador "${name}" eliminado exitosamente.`);
      void loadData();
      setTimeout(() => setSuccess(""), 4000);
    } catch {
      setError("No se pudo eliminar el operador. Inténtelo más tarde.");
    }
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError("");

    if (!formNombre.trim()) {
      setFormError("El nombre del operador es obligatorio.");
      return;
    }

    if (!selectedOperador && !formPin) {
      setFormError("El PIN es obligatorio para nuevos operadores.");
      return;
    }

    if (formPin && !/^\d{6,8}$/.test(formPin)) {
      setFormError("El PIN debe tener entre 6 y 8 dígitos numéricos.");
      return;
    }

    setSaving(true);
    try {
      if (selectedOperador) {
        await api.actualizarOperador(selectedOperador.operadorId, {
          nombre: formNombre,
          activo: formActivo,
          nuevoPin: formPin || undefined
        });
        setSuccess(`Operador "${formNombre}" modificado con éxito.`);
      } else {
        await api.crearOperador(formNombre, formPin);
        setSuccess(`Operador "${formNombre}" registrado con éxito.`);
      }
      setModalOpen(false);
      void loadData();
      setTimeout(() => setSuccess(""), 4000);
    } catch {
      setFormError("Error al sincronizar con el servidor. Valide los campos.");
    } finally {
      setSaving(false);
    }
  };

  const [filtroEstado, setFiltroEstado] = useState<string>("Habilitado");
  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Gestión de Operadores y Cajeros</h1>
        <button className="btn btn-primary" onClick={handleOpenNewModal}>
          <Plus size={16} />
          <span>Nuevo Operador</span>
        </button>
      </div>

      {success && (
        <div className="success-banner">
          <Check size={18} />
          <span>{success}</span>
        </div>
      )}

      {error && (
        <div className="error-banner" role="alert">
          <AlertCircle size={18} />
          <span>{error}</span>
        </div>
      )}

      {loading ? (
        <div className="loading-spinner-wrap">
          <div className="spinner"></div>
          <span>Consultando credenciales de operadores autorizados...</span>
        </div>
      ) : (
        <div className="table-wrap">
          <div style={{ padding: "0 16px 16px", display: "flex", gap: "8px", alignItems: "center" }}>
            <label htmlFor="filtro-estado" style={{ fontSize: "14px", color: "#666", fontWeight: "bold" }}>Filtrar por estado:</label>
            <select
              id="filtro-estado"
              value={filtroEstado}
              onChange={(e) => setFiltroEstado(e.target.value)}
              className="form-select"
              style={{ padding: "6px 12px", borderRadius: "6px", border: "1px solid #d1d5db", backgroundColor: "white", minWidth: "150px" }}
            >
              <option value="Habilitado">Habilitado</option>
              <option value="Inhabilitado">Inhabilitado</option>
              <option value="Eliminado">Eliminado</option>
            </select>
          </div>
          <table className="pos-table">
            <thead>
              <tr>
                <th style={{ width: "120px" }}>ID de Operador</th>
                <th>Nombre Completo</th>
                <th>Estado en POS</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {operadores.filter(op => {
                if (filtroEstado === "Habilitado") return op.activo && !op.eliminado;
                if (filtroEstado === "Inhabilitado") return !op.activo && !op.eliminado;
                if (filtroEstado === "Eliminado") return op.eliminado;
                return true;
              }).map((op) => (
                <tr key={op.operadorId} className={(!op.activo || op.eliminado) ? "row-inactive" : ""}>
                  <td className="font-semibold font-mono text-xs">{op.operadorId}</td>
                  <td>
                    <span className="product-display-name">{op.nombre}</span>
                  </td>
                  <td>
                    <span className={op.eliminado ? "badge-eliminado" : (op.activo ? "badge-habilitado" : "badge-deshabilitado")} style={op.eliminado ? { backgroundColor: "#fee2e2", color: "#991b1b", padding: "4px 8px", borderRadius: "9999px", fontSize: "12px", fontWeight: 500 } : {}}>
                      {op.eliminado ? "Eliminado" : (op.activo ? "Habilitado" : "Inhabilitado")}
                    </span>
                  </td>
                  <td>
                    <div className="flex gap-2">
                      <button 
                        className="btn-action-edit" 
                        onClick={() => handleOpenEditModal(op)}
                        style={{ backgroundColor: "#EFF6FF", color: "#2563EB", padding: "6px 12px", borderRadius: "6px", fontWeight: 500, border: "1px solid #BFDBFE" }}
                      >
                        Editar
                      </button>
                      <button
                        className="btn-action-delete"
                        onClick={() => handleDeleteOperador(op.operadorId, op.nombre)}
                        style={{ backgroundColor: "#FEF2F2", color: "#DC2626", padding: "6px 12px", borderRadius: "6px", fontWeight: 500, border: "1px solid #FECACA" }}
                      >
                        Eliminar
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Operator Modal Popup conforming to spec requirements */}
      {modalOpen && (
        <div className="modal-overlay" role="presentation" onClick={() => setModalOpen(false)}>
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="operador-modal-title"
            ref={dialogRef}
            tabIndex={-1}
            className="modal-box"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h2 id="operador-modal-title">
                {selectedOperador ? "Editar Operador" : "Registrar Nuevo Operador"}
              </h2>
              <button className="modal-close" onClick={() => setModalOpen(false)} aria-label="Cerrar modal">
                ✕
              </button>
            </div>

            <form onSubmit={handleSubmit} className="modal-form">
              {formError && (
                <div className="error-banner" role="alert">
                  <span>{formError}</span>
                </div>
              )}

              <label htmlFor="operador-nombre">
                Nombre Completo *
                <input
                  id="operador-nombre"
                  type="text"
                  placeholder="ej. Carlos Mendoza"
                  value={formNombre}
                  onChange={(e) => setFormNombre(e.target.value)}
                  required
                />
              </label>

              <label htmlFor="operador-pin">
                {selectedOperador
                  ? "Nuevo PIN (dejar vacío para no cambiar)"
                  : "PIN Autorizado * (6–8 dígitos)"}
                <input
                  id="operador-pin"
                  type="password"
                  inputMode="numeric"
                  maxLength={8}
                  placeholder={selectedOperador ? "••••" : "Ingrese un PIN numérico"}
                  value={formPin}
                  onChange={(e) => setFormPin(e.target.value.replace(/\D/g, ""))}
                  required={!selectedOperador}
                />
              </label>

              {selectedOperador && (
                <label htmlFor="operador-activo" className="checkbox-label">
                  <input
                    id="operador-activo"
                    type="checkbox"
                    checked={formActivo}
                    onChange={(e) => setFormActivo(e.target.checked)}
                  />
                  Habilitado para operar cajón y vender
                </label>
              )}

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setModalOpen(false)}
                >
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary" disabled={saving}>
                  {saving ? "Guardando..." : "Guardar Operador"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}