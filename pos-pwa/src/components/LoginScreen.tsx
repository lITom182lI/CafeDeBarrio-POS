import { useState, useEffect, useRef } from 'react'
import { getOperadores, validarPin, OfflineError } from '../api'
import type { OperadorDto, OperadorSession } from '../types'

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
  const pinRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    getOperadores()
      .then(ops => { setOperadores(ops); setLoading(false) })
      .catch(err => {
        if (err instanceof OfflineError) setOffline(true)
        setLoading(false)
      })
  }, [])

  async function handleLogin() {
    const id = parseInt(operadorId)
    if (!id || pin.length < 4) return
    setValidating(true)
    setError(null)
    try {
      const result = await validarPin(id, pin)
      if (result) {
        onLogin({ operadorId: result.operadorId, nombre: result.nombre })
      } else {
        setError('PIN incorrecto')
        setPin('')
        pinRef.current?.focus()
      }
    } catch (err) {
      if (err instanceof OfflineError) setOffline(true)
      else setError('Error al conectar')
    } finally {
      setValidating(false)
    }
  }

  return (
    <div className="min-h-screen bg-stone-900 flex items-center justify-center p-4">
      <div className="bg-stone-800 rounded-2xl shadow-2xl w-full max-w-sm p-8 space-y-6">
        <div className="text-center">
          <div className="text-4xl mb-2">☕</div>
          <h1 className="text-2xl font-bold text-stone-100">Café de Barrio</h1>
          <p className="text-stone-400 text-sm mt-1">Sistema de Punto de Venta</p>
        </div>

        {offline && (
          <div className="bg-amber-900/40 border border-amber-600 rounded-lg p-3 text-amber-300 text-sm">
            Sin conexión al servidor
          </div>
        )}

        {!offline && !loading && (
          <div className="space-y-4">
            <div>
              <label htmlFor="operador-id" className="block text-stone-300 text-sm mb-1">Cajero</label>
              <select
                id="operador-id"
                value={operadorId}
                onChange={e => { setOperadorId(e.target.value); setPin(''); setError(null) }}
                className="w-full bg-stone-700 text-stone-100 rounded-lg px-3 py-2 border border-stone-600 focus:outline-none focus:border-amber-500"
              >
                <option value="">— Seleccionar —</option>
                {operadores.map(op => (
                  <option key={op.operadorId} value={op.operadorId}>{op.nombre}</option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="pin-input" className="block text-stone-300 text-sm mb-1">PIN</label>
              <input
                id="pin-input"
                ref={pinRef}
                type="password"
                inputMode="numeric"
                maxLength={8}
                value={pin}
                onChange={e => { setPin(e.target.value.replace(/\D/g, '')); setError(null) }}
                onKeyDown={e => e.key === 'Enter' && handleLogin()}
                disabled={!operadorId}
                placeholder="••••"
                className="w-full bg-stone-700 text-stone-100 rounded-lg px-3 py-2 border border-stone-600 focus:outline-none focus:border-amber-500 text-center text-2xl tracking-widest"
              />
            </div>

            {error && <p className="text-red-400 text-sm text-center">{error}</p>}

            <button
              onClick={handleLogin}
              disabled={!operadorId || pin.length < 4 || validating}
              className="w-full py-3 rounded-lg font-semibold text-white transition-colors disabled:opacity-40 bg-brand"
            >
              {validating ? 'Verificando...' : 'Ingresar'}
            </button>
          </div>
        )}

        {loading && (
          <div className="text-center text-stone-400 py-4">Conectando...</div>
        )}

        {(offline || (!loading && operadores.length === 0)) && (
          <button
            onClick={() => onLogin(null)}
            className="w-full py-2 rounded-lg text-stone-400 text-sm hover:text-stone-200 transition-colors border border-stone-600 hover:border-stone-400"
          >
            Continuar sin identificar cajero
          </button>
        )}
      </div>
    </div>
  )
}
