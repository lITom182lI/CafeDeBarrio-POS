import { useState } from 'react'
import type { ComprobanteData } from '../types'
import { apiQueryDocumento } from '../api'
import { X, Search, Info } from 'lucide-react'

interface Props {
  onConfirm: (data: ComprobanteData) => void
  onCancel: () => void
}

export default function ComprobanteModal({ onConfirm, onCancel }: Props) {
  const [tipo, setTipo] = useState<'DNI' | 'RUC'>('DNI')
  const [numero, setNumero] = useState('')
  const [nombre, setNombre] = useState('')
  const [searching, setSearching] = useState(false)
  const [feedback, setFeedback] = useState<string | null>(null)

  const handleQueryDocument = async () => {
    if (numero.length < 8) return
    setSearching(true)
    setFeedback(null)
    try {
      const resp = await apiQueryDocumento(tipo, numero)
      if (resp && resp.razonSocial) {
        setNombre(resp.razonSocial)
        setFeedback(`Documento verificado exitosamente.`)
      } else {
        setFeedback('No se encontraron registros activos.')
      }
    } catch {
      setFeedback('Error de consulta, favor introduzca los datos de forma manual.')
    } finally {
      setSearching(false)
    }
  }

  const isInvalid = !numero || !nombre

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-xs p-4 animate-fadeIn">
      <div className="bg-white rounded-[24px] shadow-lg w-full max-w-md overflow-hidden border border-[#E2E8F0] flex flex-col">
        
        {/* Header */}
        <div className="bg-[#F8FAFC] px-6 py-4 border-b border-[#E2E8F0] flex items-center justify-between">
          <h2 className="text-[#1E293B] font-extrabold text-base">Comprobante Nominado</h2>
          <button
            onClick={onCancel}
            className="p-1 rounded-full text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition cursor-pointer"
          >
            <X size={20} />
          </button>
        </div>

        {/* Form Body */}
        <div className="p-6 space-y-4">
          
          {/* Document Type Selector */}
          <div>
            <label className="block text-[#1E293B] text-xs uppercase font-extrabold tracking-wider mb-2">
              Tipo de Documento
            </label>
            <div className="grid grid-cols-4 gap-1">
              {(['DNI', 'RUC'] as const).map(t => (
                <button
                  key={t}
                  type="button"
                  onClick={() => { setTipo(t); setNumero(''); setNombre('') }}
                  className={`py-2 rounded-lg text-xs font-bold transition cursor-pointer ${
                    tipo === t
                      ? 'bg-[#7C2D12] text-white shadow-2xs'
                      : 'bg-[#F8FAFC] text-[#334155] border border-[#E2E8F0] hover:bg-white'
                  }`}
                >
                  {t}
                </button>
              ))}
            </div>
          </div>

          {/* Document Number with Search/Verify */}
          <div>
            <label htmlFor="doc-number" className="block text-[#1E293B] text-xs uppercase font-extrabold tracking-wider mb-1.5">
              Número de Documento
            </label>
            <div className="flex gap-2">
              <div className="relative flex-1">
                <input
                  id="doc-number"
                  type="text"
                  maxLength={tipo === 'RUC' ? 11 : 8}
                  placeholder={tipo === 'DNI' ? '8 dígitos' : tipo === 'RUC' ? '11 dígitos' : 'Alfanumérico'}
                  value={numero}
                  onChange={e => setNumero(e.target.value.replace(/[^\w]/g, ''))}
                  className="w-full bg-[#F8FAFC] border border-[#E2E8F0] rounded-lg px-3 py-2 text-[#1E293B] font-semibold text-xs focus:outline-none focus:border-[#7C2D12] placeholder-slate-450"
                />
              </div>
              <button
                type="button"
                onClick={handleQueryDocument}
                disabled={numero.length < 8}
                className="px-4 py-2 bg-[#F8FAFC] border border-[#E2E8F0] hover:bg-white text-[#334155] font-bold rounded-lg text-xs flex items-center gap-1.5 transition disabled:opacity-40 cursor-pointer"
              >
                {searching ? (
                  <span className="w-4 h-4 border-2 border-[#7C2D12]/20 border-t-[#7C2D12] rounded-full animate-spin" />
                ) : (
                  <Search size={14} />
                )}
                <span>Consultar</span>
              </button>
            </div>
            {feedback && (
              <p className="text-[10px] text-slate-500 font-medium mt-1 flex items-center gap-1">
                <Info size={10} />
                <span>{feedback}</span>
              </p>
            )}
          </div>

          {/* Customer name / Social reason */}
          <div>
            <label htmlFor="customer-name" className="block text-[#1E293B] text-xs uppercase font-extrabold tracking-wider mb-1.5">
              Nombre de Cliente o Razón Social
            </label>
            <input
              id="customer-name"
              type="text"
              placeholder="Ej. Público General, Jesús Peralta"
              value={nombre}
              onChange={e => setNombre(e.target.value)}
              className="w-full bg-[#F8FAFC] border border-[#E2E8F0] rounded-lg px-3 py-2 text-[#1E293B] font-semibold text-xs focus:outline-none focus:border-[#7C2D12] placeholder-slate-450"
            />
          </div>

        </div>

        {/* Footer */}
        <div className="bg-[#F8FAFC] px-6 py-4 border-t border-[#E2E8F0] flex gap-2 justify-end">
          <button
            onClick={onCancel}
            className="px-4 py-2 border border-[#E2E8F0] rounded-lg text-xs font-bold text-[#334155]/80 hover:bg-white transition cursor-pointer"
          >
            Cancelar
          </button>
          <button
            onClick={() => onConfirm({ tipoDocumento: tipo, numeroDocumento: numero, razonSocial: nombre })}
            disabled={isInvalid}
            className="px-5 py-2 bg-[#7C2D12] hover:bg-[#6b250e] disabled:opacity-40 text-white font-extrabold rounded-lg text-xs transition shadow-2xs cursor-pointer"
          >
            Confirmar Datos
          </button>
        </div>

      </div>
    </div>
  )
}
