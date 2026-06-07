import { useState } from 'react'
import type { ComprobanteData } from '../types'

interface Props {
  onConfirm: (data: ComprobanteData) => void
  onCancel: () => void
}

const TIPOS = ['DNI', 'RUC', 'CE', 'Pasaporte'] as const
type TipoDoc = typeof TIPOS[number]

function maxLen(tipo: TipoDoc) {
  if (tipo === 'DNI') return 8
  if (tipo === 'RUC') return 11
  return 20
}

function validate(tipo: TipoDoc, numero: string, nombre: string): string | null {
  if (!numero.trim()) return 'Ingrese el número de documento'
  if (tipo === 'DNI' && numero.length !== 8) return 'DNI debe tener exactamente 8 dígitos'
  if (tipo === 'RUC' && numero.length !== 11) return 'RUC debe tener exactamente 11 dígitos'
  if (!nombre.trim()) return 'Ingrese el nombre o razón social'
  return null
}

export default function ComprobanteModal({ onConfirm, onCancel }: Props) {
  const [tipo, setTipo] = useState<TipoDoc>('DNI')
  const [numero, setNumero] = useState('')
  const [nombre, setNombre] = useState('')
  const [error, setError] = useState<string | null>(null)

  function handleConfirm() {
    const err = validate(tipo, numero, nombre)
    if (err) { setError(err); return }
    onConfirm({ tipoDocumento: tipo, numeroDocumento: numero.trim().toUpperCase(), razonSocial: nombre.trim().toUpperCase() })
  }

  return (
    <div className="fixed inset-0 bg-black/70 flex items-center justify-center z-50 p-4">
      <div className="bg-stone-800 rounded-xl shadow-2xl w-full max-w-sm p-6 space-y-4">
        <h2 className="text-stone-100 font-semibold text-lg">Datos para boleta</h2>

        <div>
          <label className="block text-stone-400 text-xs mb-2 uppercase tracking-wide">Tipo documento</label>
          <div className="flex gap-2 flex-wrap">
            {TIPOS.map(t => (
              <button
                key={t}
                onClick={() => { setTipo(t); setNumero(''); setError(null) }}
                className={`px-3 py-1 rounded-full text-sm font-medium transition-colors ${
                  tipo === t
                    ? 'bg-amber-600 text-white'
                    : 'bg-stone-700 text-stone-300 hover:bg-stone-600'
                }`}
              >
                {t}
              </button>
            ))}
          </div>
        </div>

        <div>
          <label htmlFor="numero-doc" className="block text-stone-400 text-xs mb-1 uppercase tracking-wide">
            Número {tipo === 'DNI' ? '(8 dígitos)' : tipo === 'RUC' ? '(11 dígitos)' : ''}
          </label>
          <input
            id="numero-doc"
            type="text"
            inputMode={tipo !== 'Pasaporte' ? 'numeric' : 'text'}
            maxLength={maxLen(tipo)}
            value={numero}
            onChange={e => {
              const v = tipo !== 'Pasaporte' ? e.target.value.replace(/\D/g, '') : e.target.value
              setNumero(v); setError(null)
            }}
            className="w-full bg-stone-700 text-stone-100 rounded-lg px-3 py-2 border border-stone-600 focus:outline-none focus:border-amber-500"
          />
        </div>

        <div>
          <label htmlFor="nombre-doc" className="block text-stone-400 text-xs mb-1 uppercase tracking-wide">
            {tipo === 'RUC' ? 'Razón social' : 'Nombre completo'}
          </label>
          <input
            id="nombre-doc"
            type="text"
            value={nombre}
            onChange={e => { setNombre(e.target.value); setError(null) }}
            className="w-full bg-stone-700 text-stone-100 rounded-lg px-3 py-2 border border-stone-600 focus:outline-none focus:border-amber-500"
          />
        </div>

        {error && <p className="text-red-400 text-sm">{error}</p>}

        <div className="flex gap-3 pt-1">
          <button
            onClick={onCancel}
            className="flex-1 py-2 rounded-lg border border-stone-600 text-stone-300 hover:bg-stone-700 transition-colors"
          >
            Cancelar
          </button>
          <button
            onClick={handleConfirm}
            className="flex-1 py-2 rounded-lg bg-amber-600 text-white font-semibold hover:bg-amber-500 transition-colors"
          >
            Confirmar
          </button>
        </div>
      </div>
    </div>
  )
}
