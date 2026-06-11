import { useState, useEffect, useRef } from 'react'
import { getOperadores, validarPin, OfflineError, setOperadorToken, getTurnoActivo } from '../api'
import type { OperadorDto, OperadorSession } from '../types'
import { saveCatalogOperadores, getCatalogOperadores } from '../offline'
import { Coffee, Key, AlertCircle, WifiOff, CornerDownLeft, Delete, Sun, Moon, Eraser } from 'lucide-react'

interface Props {
  onLogin: (session: OperadorSession | null) => void
}

export default function LoginScreen({ onLogin }: Props) {
  const [operadores, setOperadores] = useState<OperadorDto[]>([])
  const [operadorId, setOperadorId] = useState('')
  const [pin, setPin] = useState('')
  const [loading, setLoading] = useState(true)
  const [offline, setOffline] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [validating, setValidating] = useState(false)
  const [isDark, setIsDark] = useState(false)
  const pinRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    // Check initial dark mode state
    if (document.documentElement.classList.contains("dark")) {
      setIsDark(true)
    } else {
      setIsDark(false)
    }

    getOperadores()
      .then(ops => {
        setOperadores(ops)
        setLoading(false)
        void saveCatalogOperadores(ops)
      })
      .catch(err => {
        if (err instanceof OfflineError) {
          setOffline(true)
          // Intenta cargar operadores de la base local (catálogo cacheado)
          getCatalogOperadores().then(cachedOps => {
            setOperadores(cachedOps)
            if (cachedOps.length === 0) {
              // DT-05: Catálogo offline vacío — nunca se hizo una sesión online previa
              setError('Sin conexión y sin catálogo local. Conéctate al menos una vez para habilitar el modo offline.')
            }
          })
        } else {
          setError('Error al conectar con el servidor de la sucursal.')
        }
        setLoading(false)
      })
  }, [])

  async function handleLogin() {
    const id = parseInt(operadorId)
    if (!id || pin.length < 6) return
    setValidating(true)
    setError(null)
    try {
      const result = await validarPin(id, pin)
      if (result) {
        setOperadorToken(result.token)
        
        // Verificar turno activo si estamos online
        try {
          const turno = await getTurnoActivo();
          if (!turno) {
            setError('No hay un turno de caja abierto. Solicite al administrador que abra el turno desde el Dashboard antes de ingresar.');
            setPin('');
            pinRef.current?.focus();
            setOperadorToken(null);
            return;
          }
        } catch (turnoErr) {
          console.warn('No se pudo verificar el turno activo, procediendo por precaución...', turnoErr);
        }

        onLogin({ operadorId: result.operadorId, nombre: result.nombre, token: result.token })
      } else {
        setError('PIN de seguridad incorrecto')
        setPin('')
        pinRef.current?.focus()
      }
    } catch (err) {
      if (err instanceof OfflineError) {
        setOffline(true)
        // Permitir inicio offline limitado si el operador ya existe localmente
        const op = operadores.find(o => o.operadorId === id)
        if (op) {
          setError('Modo Sin Conexión: Iniciando sesión de Cajero offline...')
          setTimeout(() => {
            onLogin({ operadorId: id, nombre: op.nombre, token: null })
          }, 1000)
        } else {
          setError('PIN de seguridad no validado en modo offline')
        }
      } else {
        setError('Error al conectar con la sucursal')
      }
    } finally {
      setValidating(false)
    }
  }

  // Tactile PIN Numpad handlers for Tablet POS environment
  const pressNumber = (num: string) => {
    if (!operadorId || pin.length >= 8) return
    setError(null)
    setPin(prev => prev + num)
  }

  const deleteLastDigit = () => {
    setPin(prev => prev.slice(0, -1))
  }

  const clearPin = () => {
    setPin('')
  }

  const toggleDark = () => {
    if (isDark) {
      document.documentElement.classList.remove("dark")
      localStorage.setItem('theme', 'light')
      setIsDark(false)
    } else {
      document.documentElement.classList.add("dark")
      localStorage.setItem('theme', 'dark')
      setIsDark(true)
    }
  }

  return (
    <div className="min-h-screen bg-[#F8FAFC] dark:bg-gray-900 flex items-center justify-center p-4 transition-colors">
      {/* Absolute Header branding */}
      <div className="absolute top-6 left-6 flex items-center gap-2.5">
        <div className="bg-[#f97316] w-[36px] h-[36px] rounded-[12px] flex items-center justify-center text-white shadow-xs border border-[#f97316]/20 transition-colors">
          <Coffee size={20} strokeWidth={2.5} />
        </div>
        <div>
          <h2 className="text-[#1E293B] dark:text-gray-100 font-extrabold text-xs tracking-wider uppercase transition-colors">Café de Barrio</h2>
          <p className="text-[#7C2D12] dark:text-orange-500 text-[9px] tracking-wider uppercase font-bold transition-colors">Terminal POS PWA</p>
        </div>
      </div>

      {/* Dark Mode Toggle */}
      <div className="absolute top-6 right-6">
        <button
          onClick={toggleDark}
          className="p-2.5 rounded-xl bg-white dark:bg-gray-800 text-slate-400 hover:text-[#7C2D12] dark:hover:text-orange-500 shadow-sm border border-[#E2E8F0] dark:border-gray-700 transition-colors cursor-pointer"
          title={isDark ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro'}
        >
          {isDark ? <Sun size={18} /> : <Moon size={18} />}
        </button>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-3xl shadow-sm border border-[#E2E8F0] dark:border-gray-700 w-full max-w-4xl grid md:grid-cols-12 overflow-hidden transition-colors">
        
        {/* Left welcome section (Visual Brand context) */}
        <div className="md:col-span-5 bg-[#F8FAFC] dark:bg-gray-800/50 p-8 flex flex-col justify-between text-[#334155] dark:text-gray-300 relative min-h-[300px] md:min-h-auto border-r border-[#E2E8F0] dark:border-gray-700 transition-colors">
          <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-[#7C2D12]/5 dark:from-orange-500/10 via-transparent to-transparent pointer-events-none" />
          
          <div className="relative space-y-2">
            <span className="bg-[#7C2D12]/10 dark:bg-orange-500/20 text-[#7C2D12] dark:text-orange-400 text-[9px] font-extrabold px-3 py-1 rounded-full uppercase tracking-widest border border-[#7C2D12]/20 dark:border-orange-500/30 transition-colors">
              Módulo Sucursal
            </span>
            <h1 className="text-4xl font-sans font-extrabold tracking-tight pt-3 text-[#1E293B] dark:text-gray-100 transition-colors">Inicia tu Jornada</h1>
            <p className="text-[#334155]/95 dark:text-gray-400 text-xs leading-relaxed max-w-xs mt-1 transition-colors">
              Ingresa tu código de cajero u operador y tu código PIN personal de seguridad para acceder a la terminal de ventas.
            </p>
          </div>

          <div className="relative space-y-4 pt-6 mt-12 md:mt-0">
            {/* Real Offline State */}
            {offline ? (
              <div className="bg-amber-50/60 dark:bg-amber-900/20 border border-amber-900/10 dark:border-amber-700/30 rounded-2xl p-4 flex items-start gap-3 backdrop-blur-xs transition-colors">
                <WifiOff className="text-[#7C2D12] dark:text-amber-500 flex-shrink-0 mt-0.5 animate-bounce" size={20} />
                <div>
                  <p className="font-extrabold text-[#7C2D12] dark:text-amber-400 text-sm transition-colors">Modo Sin Conexión</p>
                  <p className="text-[#334155]/85 dark:text-amber-200/70 text-xs leading-normal transition-colors">
                    La pasarela opera offline. Se guardarán las comandas en local y se sincronizarán al conectar.
                  </p>
                </div>
              </div>
            ) : (
              <div className="bg-[#F8FAFC] dark:bg-gray-800/80 border border-[#E2E8F0] dark:border-gray-700 rounded-2xl p-4 flex items-center gap-3 transition-colors">
                <div className="w-2 h-2 bg-[#10B981] dark:bg-emerald-400 rounded-full animate-pulse" />
                <p className="text-xs text-[#334155] dark:text-gray-300 font-extrabold transition-colors">Terminal conectada - Sede Barranco</p>
              </div>
            )}
          </div>
        </div>

        {/* Right input section with tactile numpad */}
        <div className="md:col-span-7 p-8 md:p-10 flex flex-col justify-center space-y-6 bg-white dark:bg-gray-800 transition-colors">
          <div className="space-y-1.5">
            <h3 className="text-[#1E293B] dark:text-gray-100 text-xl font-extrabold tracking-tight transition-colors">Identificación de Cajero</h3>
            <p className="text-[#334155]/85 dark:text-gray-400 text-xs font-semibold transition-colors">Selecciona tu usuario de la lista e introduce tu PIN</p>
          </div>

          {loading ? (
            <div className="py-20 flex flex-col items-center justify-center space-y-3">
              <div className="w-10 h-10 border-4 border-[#7C2D12] dark:border-orange-500 border-t-white dark:border-t-transparent rounded-full animate-spin transition-colors" />
              <p className="text-slate-500 dark:text-gray-400 text-sm animate-pulse transition-colors">Cargando catálogo de operadores de Barrio...</p>
            </div>
          ) : offline && operadores.length === 0 ? (
            <div className="py-16 flex flex-col items-center justify-center space-y-4 text-center px-4">
              <WifiOff size={40} className="text-[#7C2D12] dark:text-orange-500 animate-pulse transition-colors" />
              <div>
                <p className="text-[#1E293B] dark:text-gray-200 font-extrabold text-sm transition-colors">Sin catálogo offline disponible</p>
                <p className="text-[#334155]/70 dark:text-gray-400 text-xs mt-1 leading-relaxed max-w-xs transition-colors">
                  Para operar sin conexión, esta terminal necesita haber iniciado sesión al menos una vez estando en línea.
                  Conecta el dispositivo a la red de la sucursal e intenta de nuevo.
                </p>
              </div>
              <button
                onClick={() => window.location.reload()}
                className="mt-2 bg-[#7C2D12] dark:bg-orange-600 text-white text-xs font-extrabold px-5 py-2.5 rounded-xl hover:bg-[#6b1f0a] dark:hover:bg-orange-700 transition cursor-pointer"
              >
                Reintentar conexión
              </button>
            </div>
          ) : (
            <div className="grid md:grid-cols-2 gap-6 items-start">
              {/* Select cajero */}
              <div className="space-y-4">
                <div>
                  <label htmlFor="operador-select" className="block text-[#1E293B] dark:text-gray-300 text-xs uppercase font-extrabold tracking-wider mb-2 transition-colors">
                    Cajero Activo
                  </label>
                  <select
                    id="operador-select"
                    value={operadorId}
                    onChange={e => { setOperadorId(e.target.value); setPin(''); setError(null) }}
                    className="w-full bg-[#F8FAFC] dark:bg-gray-900 text-[#1E293B] dark:text-gray-100 rounded-xl px-4 py-3 border border-[#E2E8F0] dark:border-gray-700 focus:outline-none focus:border-[#7C2D12] dark:focus:border-orange-500 text-xs font-bold transition-colors cursor-pointer"
                  >
                    <option value="" className="bg-white dark:bg-gray-800 text-[#334155]/60 dark:text-gray-500">— Seleccionar Cajero —</option>
                    {operadores.filter(op => op.activo).map(op => (
                      <option key={op.operadorId} value={op.operadorId} className="bg-white dark:bg-gray-800 text-[#1E293B] dark:text-gray-200">
                        [#{op.operadorId.toString().padStart(2, '0')}] {op.nombre}
                      </option>
                    ))}
                  </select>
                </div>

                {/* PIN Display box */}
                <div>
                  <label htmlFor="pin-display" className="block text-[#1E293B] dark:text-gray-300 text-xs uppercase font-extrabold tracking-wider mb-2 transition-colors">
                    PIN de Seguridad
                  </label>
                  <div className="relative">
                    <input
                      id="pin-display"
                      ref={pinRef}
                      type="password"
                      inputMode="none"
                      maxLength={8}
                      readOnly
                      value={pin}
                      placeholder="••••"
                      className="w-full bg-[#F8FAFC] dark:bg-gray-900 text-[#7C2D12] dark:text-orange-500 border border-[#E2E8F0] dark:border-gray-700 rounded-xl px-4 py-3 focus:outline-none text-center text-3xl font-extrabold tracking-widest placeholder-slate-300 dark:placeholder-gray-600 transition-colors"
                    />
                  </div>
                </div>

                {error && (
                  <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800/30 rounded-xl p-3 flex items-start gap-2 text-red-700 dark:text-red-400 text-xs animate-headShake font-medium transition-colors">
                    <AlertCircle className="flex-shrink-0 text-red-500 dark:text-red-400" size={16} />
                    <p>{error}</p>
                  </div>
                )}

                <button
                  onClick={handleLogin}
                  disabled={!operadorId || pin.length < 6 || validating}
                  className="w-full py-3.5 bg-[#7C2D12] dark:bg-orange-600 hover:bg-[#63220e] dark:hover:bg-orange-700 disabled:opacity-30 disabled:hover:bg-[#7C2D12] dark:disabled:hover:bg-orange-600 text-white font-extrabold rounded-xl text-xs transition shadow-sm active:scale-95 cursor-pointer flex items-center justify-center gap-2"
                >
                  {validating ? (
                    <>
                      <span className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                      <span>Verificando PIN...</span>
                    </>
                  ) : (
                    <>
                      <CornerDownLeft size={14} />
                      <span>Ingresar a Terminal</span>
                    </>
                  )}
                </button>
                
                {(offline || operadores.length === 0) && (
                  <button
                    onClick={() => onLogin(null)}
                    className="w-full py-2.5 rounded-xl text-[#334155] dark:text-gray-300 text-xs hover:text-[#7C2D12] dark:hover:text-orange-400 hover:bg-[#F8FAFC] dark:hover:bg-gray-700 transition-colors border border-[#E2E8F0] dark:border-gray-600 border-dashed cursor-pointer font-semibold"
                  >
                    Ingresar como Cajero Genérico
                  </button>
                )}
              </div>

              {/* Tactile Numpad */}
              <div className="bg-[#F8FAFC] dark:bg-gray-800/80 p-4 rounded-2xl border border-[#E2E8F0] dark:border-gray-700 flex flex-col space-y-2 transition-colors">
                <div className="grid grid-cols-3 gap-2">
                  {['1', '2', '3', '4', '5', '6', '7', '8', '9'].map(num => (
                    <button
                      key={num}
                      type="button"
                      disabled={!operadorId}
                      onClick={() => pressNumber(num)}
                      className="aspect-square bg-white dark:bg-gray-900 hover:bg-[#F8FAFC] dark:hover:bg-gray-700 hover:text-[#7C2D12] dark:hover:text-orange-500 active:scale-95 disabled:opacity-20 text-[#1E293B] dark:text-gray-200 font-bold text-lg rounded-xl flex items-center justify-center transition-all border border-[#E2E8F0] dark:border-gray-700 shadow-2xs cursor-pointer"
                    >
                      {num}
                    </button>
                  ))}
                  
                  {/* Back/Clear button */}
                  <button
                    type="button"
                    aria-label="Borrar último dígito"
                    disabled={!operadorId || pin.length === 0}
                    onClick={deleteLastDigit}
                    className="aspect-square bg-white dark:bg-gray-900 hover:bg-[#F8FAFC] dark:hover:bg-gray-700 text-slate-400 dark:text-slate-500 hover:text-[#7C2D12] dark:hover:text-orange-500 active:scale-95 disabled:opacity-20 rounded-xl flex items-center justify-center transition-all border border-[#E2E8F0] dark:border-gray-700 shadow-2xs cursor-pointer"
                  >
                    <Delete size={20} />
                  </button>

                  <button
                    type="button"
                    disabled={!operadorId}
                    onClick={() => pressNumber('0')}
                    className="aspect-square bg-white dark:bg-gray-900 hover:bg-[#F8FAFC] dark:hover:bg-gray-700 hover:text-[#7C2D12] dark:hover:text-orange-500 active:scale-95 disabled:opacity-20 text-[#1E293B] dark:text-gray-200 font-bold text-lg rounded-xl flex items-center justify-center transition-all border border-[#E2E8F0] dark:border-gray-700 shadow-2xs cursor-pointer"
                  >
                    0
                  </button>

                  {/* Clear All button directly */}
                  <button
                    type="button"
                    aria-label="Limpiar todo"
                    disabled={!operadorId || pin.length === 0}
                    onClick={clearPin}
                    className="aspect-square bg-white dark:bg-gray-900 hover:bg-[#F8FAFC] dark:hover:bg-gray-700 text-slate-400 dark:text-slate-500 hover:text-[#7C2D12] dark:hover:text-orange-500 active:scale-95 disabled:opacity-20 rounded-xl flex items-center justify-center transition-all border border-[#E2E8F0] dark:border-gray-700 shadow-2xs cursor-pointer"
                  >
                    <Eraser size={20} />
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>

      </div>
    </div>
  )
}
