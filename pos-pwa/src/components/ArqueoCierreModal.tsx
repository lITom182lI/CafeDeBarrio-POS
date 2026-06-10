import { useState, useEffect, useRef } from "react"
import { getTurnoActivo, cerrarTurno, validarPin } from "../api"
import type { OperadorSession } from "../types"
import { ShieldCheck, AlertCircle, Key, Delete, DollarSign } from "lucide-react"

interface Props {
  session: OperadorSession
  onClose: () => void
  onSuccess: () => void
}

export default function ArqueoCierreModal({ session, onClose, onSuccess }: Props) {
  const [step, setStep] = useState<"loading" | "arqueo" | "pin">("loading")
  const [turnoId, setTurnoId] = useState<number | null>(null)
  
  // Arqueo State
  const [montoCierto, setMontoCierto] = useState("")
  const [observaciones, setObservaciones] = useState("")
  
  // PIN State
  const [pin, setPin] = useState("")
  const pinRef = useRef<HTMLInputElement>(null)

  const [error, setError] = useState<string | null>(null)
  const [processing, setProcessing] = useState(false)

  useEffect(() => {
    // Fetch active shift on mount
    getTurnoActivo()
      .then(turno => {
        if (!turno) {
          setError("No se detectó un turno activo. Si ya cerró caja, cierre sesión omitiendo el arqueo.")
          setStep("arqueo") // Permit skip
        } else {
          setTurnoId(turno.turnoId)
          setStep("arqueo")
        }
      })
      .catch(() => {
        setError("Error al conectar con la sucursal para verificar el turno.")
        setStep("arqueo")
      })
  }, [])

  useEffect(() => {
    if (step === "pin") {
      pinRef.current?.focus()
    }
  }, [step])

  const handleArqueoSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!turnoId) {
      // If there's no active shift detected (e.g. offline or already closed), just proceed to logout
      onSuccess()
      return
    }
    
    const amount = parseFloat(montoCierto)
    if (isNaN(amount) || amount < 0) {
      setError("Ingrese un monto arqueado válido.")
      return
    }
    setError(null)
    setStep("pin")
  }

  const handleCierreFinal = async () => {
    if (pin.length < 6) return
    setProcessing(true)
    setError(null)

    try {
      // 1. Validate PIN
      const valid = await validarPin(session.operadorId, pin)
      if (!valid) {
        setError("PIN de seguridad incorrecto.")
        setPin("")
        setProcessing(false)
        return
      }

      // 2. Cerrar turno
      const amount = parseFloat(montoCierto)
      await cerrarTurno(turnoId!, amount, observaciones)
      
      // 3. Éxito
      onSuccess()
    } catch (err: any) {
      const msg = err.message || "Error al cerrar el turno. Compruebe la conexión."
      if (msg.includes('OfflineError') || msg.includes('401')) {
        setError("Operación rechazada o sin conexión. Verifique red.")
      } else {
        setError(msg)
      }
      setPin("")
      setProcessing(false)
    }
  }

  // Tactile Numpad
  const pressNumber = (num: string) => {
    if (pin.length >= 8) return
    setError(null)
    setPin(prev => prev + num)
  }
  const deleteLastDigit = () => setPin(prev => prev.slice(0, -1))
  const clearPin = () => setPin("")

  return (
    <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-sm flex items-center justify-center p-4">
      <div className="bg-white rounded-3xl w-full max-w-lg shadow-xl overflow-hidden border border-slate-200">
        
        <div className="bg-slate-50 border-b border-slate-200 px-6 py-5 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="bg-amber-100 text-amber-700 p-2 rounded-xl">
              <ShieldCheck size={24} />
            </div>
            <div>
              <h2 className="text-lg font-extrabold text-slate-800 leading-tight">Acción de Arqueo y Cierre</h2>
              <p className="text-xs text-slate-500 font-medium">Validación obligatoria de caja</p>
            </div>
          </div>
        </div>

        <div className="p-6">
          {step === "loading" ? (
            <div className="py-12 flex flex-col items-center justify-center">
              <div className="w-8 h-8 border-4 border-amber-600 border-t-white rounded-full animate-spin mb-4" />
              <p className="text-slate-500 text-sm font-semibold">Verificando estatus de caja...</p>
            </div>
          ) : step === "arqueo" ? (
            <form onSubmit={handleArqueoSubmit} className="space-y-6">
              {error && (
                <div className="bg-rose-50 border border-rose-200 text-rose-700 px-4 py-3 rounded-xl text-sm flex items-start gap-2">
                  <AlertCircle size={18} className="flex-shrink-0 mt-0.5" />
                  <p>{error}</p>
                </div>
              )}

              {!turnoId && !error ? (
                <div className="bg-emerald-50 border border-emerald-200 text-emerald-700 px-4 py-3 rounded-xl text-sm flex items-start gap-2">
                  <ShieldCheck size={18} className="flex-shrink-0 mt-0.5" />
                  <p>No se requiere arqueo activo. Puede cerrar sesión.</p>
                </div>
              ) : (
                <>
                  <div>
                    <label className="block text-slate-700 text-xs font-bold uppercase tracking-wide mb-2">
                      Monto Físico Arqueado (S/.)
                    </label>
                    <div className="relative">
                      <span className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400">
                        <DollarSign size={18} />
                      </span>
                      <input
                        type="number"
                        step="0.01"
                        min="0"
                        autoFocus
                        required
                        placeholder="0.00"
                        className="w-full bg-slate-50 border border-slate-200 rounded-xl py-3 pl-10 pr-4 text-slate-800 font-mono font-bold focus:outline-none focus:border-amber-500 focus:ring-1 focus:ring-amber-500 transition-colors"
                        value={montoCierto}
                        onChange={e => { setMontoCierto(e.target.value); setError(null) }}
                      />
                    </div>
                    <p className="text-[11px] text-slate-500 mt-2 font-medium">Ingrese el conteo billete por billete retirado del cajón de efectivo.</p>
                  </div>

                  <div>
                    <label className="block text-slate-700 text-xs font-bold uppercase tracking-wide mb-2">
                      Observaciones (Opcional)
                    </label>
                    <textarea
                      rows={2}
                      style={{ resize: "none" }}
                      className="w-full bg-slate-50 border border-slate-200 rounded-xl p-3 text-slate-800 text-sm focus:outline-none focus:border-amber-500 focus:ring-1 focus:ring-amber-500 transition-colors resize-none"
                      placeholder="Indique justificación si percibe sobrantes o faltantes..."
                      value={observaciones}
                      onChange={e => setObservaciones(e.target.value)}
                    />
                  </div>
                </>
              )}

              <div className="flex gap-3 pt-2">
                <button
                  type="button"
                  onClick={onClose}
                  className="flex-1 py-3 px-4 rounded-xl font-bold text-slate-600 bg-slate-100 hover:bg-slate-200 transition-colors"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="flex-1 py-3 px-4 rounded-xl font-bold text-white bg-amber-600 hover:bg-amber-700 shadow-sm transition-colors"
                >
                  {!turnoId ? "Cerrar Sesión" : "Confirmar Arqueo"}
                </button>
              </div>
            </form>
          ) : (
            <div className="space-y-6">
              <div className="text-center space-y-1">
                <h3 className="text-slate-800 font-bold text-lg">Verificación de Identidad</h3>
                <p className="text-slate-500 text-sm">Ingrese su PIN de seguridad para firmar el cierre</p>
              </div>

              {error && (
                <div className="bg-rose-50 border border-rose-200 text-rose-700 px-4 py-3 rounded-xl text-sm flex items-center justify-center gap-2">
                  <AlertCircle size={16} />
                  <p className="font-semibold">{error}</p>
                </div>
              )}

              <div className="relative max-w-[200px] mx-auto">
                <input
                  ref={pinRef}
                  type="password"
                  readOnly
                  value={pin}
                  placeholder="••••"
                  className="w-full bg-slate-50 border border-slate-200 rounded-xl py-3 text-center text-3xl font-extrabold tracking-widest text-amber-700 outline-none"
                />
                {pin.length > 0 && (
                  <button
                    onClick={clearPin}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 text-[10px] font-bold uppercase"
                  >
                    Limpiar
                  </button>
                )}
              </div>

              {/* Tactile Numpad */}
              <div className="bg-slate-50 p-4 rounded-2xl border border-slate-200 flex flex-col max-w-[280px] mx-auto">
                <div className="grid grid-cols-3 gap-2">
                  {['1', '2', '3', '4', '5', '6', '7', '8', '9'].map(num => (
                    <button
                      key={num}
                      type="button"
                      onClick={() => pressNumber(num)}
                      disabled={processing}
                      className="aspect-square bg-white hover:bg-slate-100 text-slate-800 font-bold text-lg rounded-xl flex items-center justify-center border border-slate-200 shadow-sm transition active:scale-95 disabled:opacity-50"
                    >
                      {num}
                    </button>
                  ))}
                  
                  <button
                    type="button"
                    onClick={deleteLastDigit}
                    disabled={pin.length === 0 || processing}
                    className="aspect-square bg-white hover:bg-slate-100 text-slate-400 hover:text-slate-700 rounded-xl flex items-center justify-center border border-slate-200 shadow-sm transition active:scale-95 disabled:opacity-50"
                  >
                    <Delete size={20} />
                  </button>

                  <button
                    type="button"
                    onClick={() => pressNumber('0')}
                    disabled={processing}
                    className="aspect-square bg-white hover:bg-slate-100 text-slate-800 font-bold text-lg rounded-xl flex items-center justify-center border border-slate-200 shadow-sm transition active:scale-95 disabled:opacity-50"
                  >
                    0
                  </button>

                  <button
                    type="button"
                    onClick={handleCierreFinal}
                    disabled={pin.length < 6 || processing}
                    className="aspect-square bg-amber-100 hover:bg-amber-600 hover:text-white text-amber-700 font-bold rounded-xl flex items-center justify-center border border-amber-200 shadow-sm transition active:scale-95 disabled:opacity-50"
                  >
                    {processing ? (
                      <div className="w-5 h-5 border-2 border-current border-t-transparent rounded-full animate-spin" />
                    ) : (
                      <Key size={20} />
                    )}
                  </button>
                </div>
              </div>

              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={() => { setStep("arqueo"); setPin(""); setError(null) }}
                  disabled={processing}
                  className="flex-1 py-3 px-4 rounded-xl font-bold text-slate-600 hover:text-slate-800 bg-transparent transition-colors"
                >
                  Volver al Arqueo
                </button>
              </div>

            </div>
          )}
        </div>
      </div>
    </div>
  )
}
