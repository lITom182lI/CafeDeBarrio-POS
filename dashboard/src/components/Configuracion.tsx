import { useState, type FormEvent } from "react";
import { KeyRound, ShieldAlert, CheckCircle, Eye, EyeOff } from "lucide-react";
import { api } from "../api/client";

export function Configuracion() {
  const [currentPassword, setCurrentPassword] = useState<string>("");
  const [newPassword, setNewPassword] = useState<string>("");
  const [confirmPassword, setConfirmPassword] = useState<string>("");

  const [showCurrent, setShowCurrent] = useState<boolean>(false);
  const [showNew, setShowNew] = useState<boolean>(false);
  const [showConfirm, setShowConfirm] = useState<boolean>(false);

  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>("");
  const [success, setSuccess] = useState<string>("");

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!currentPassword) {
      setError("Por favor, ingrese su contraseña actual.");
      return;
    }

    if (newPassword.length < 4) {
      setError("La nueva contraseña debe tener al menos 4 caracteres.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("La nueva contraseña y su confirmación no coinciden.");
      return;
    }

    setLoading(true);
    try {
      await api.cambiarContrasena(currentPassword, newPassword);
      setSuccess("Su contraseña ha sido actualizada con éxito.");
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
    } catch (err: any) {
      setError(
        err.message || "No se pudo actualizar la contraseña. Verifique que la contraseña actual sea correcta."
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h1 className="page-title">Configuración</h1>
        </div>
      </div>

      <div className="max-w-md mx-auto mt-8">
        <div className="card">
          <div className="flex items-center gap-2 border-b border-gray-100 pb-4 mb-6">
            <KeyRound size={20} className="text-amber-800" />
            <h2 className="text-lg font-bold text-gray-900 font-sans tracking-tight">Cambiar Contraseña</h2>
          </div>

          {success && (
            <div className="success-banner mb-6" role="status">
              <CheckCircle size={18} />
              <span>{success}</span>
            </div>
          )}

          {error && (
            <div className="error-banner mb-6" role="alert">
              <ShieldAlert size={18} />
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Contraseña Actual */}
            <div className="select-group">
              <label className="select-grouplabel" htmlFor="current-pw">Contraseña Actual</label>
              <div className="relative">
                <input
                  id="current-pw"
                  type={showCurrent ? "text" : "password"}
                  className="caja-input w-full pr-10"
                  placeholder="••••••••"
                  value={currentPassword}
                  onChange={(e) => setCurrentPassword(e.target.value)}
                  disabled={loading}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowCurrent(!showCurrent)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 focus:outline-none"
                >
                  {showCurrent ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            {/* Nueva Contraseña */}
            <div className="select-group">
              <label className="select-grouplabel" htmlFor="new-pw">Nueva Contraseña</label>
              <div className="relative">
                <input
                  id="new-pw"
                  type={showNew ? "text" : "password"}
                  className="caja-input w-full pr-10"
                  placeholder="Mínimo 4 caracteres"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  disabled={loading}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowNew(!showNew)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 focus:outline-none"
                >
                  {showNew ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            {/* Confirmar Nueva Contraseña */}
            <div className="select-group">
              <label className="select-grouplabel" htmlFor="confirm-pw">Confirmar Nueva Contraseña</label>
              <div className="relative">
                <input
                  id="confirm-pw"
                  type={showConfirm ? "text" : "password"}
                  className="caja-input w-full pr-10"
                  placeholder="Repita la nueva contraseña"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  disabled={loading}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowConfirm(!showConfirm)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 focus:outline-none"
                >
                  {showConfirm ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            <div className="pt-2">
              <button
                type="submit"
                className="btn btn-primary w-full justify-center"
                disabled={loading}
              >
                {loading ? "Actualizando contraseña..." : "Actualizar Contraseña"}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}