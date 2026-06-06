export type Periodo = 'dia' | 'semana' | 'mes'

interface Props { value: Periodo; onChange: (p: Periodo) => void }

export function PeriodoSelector({ value, onChange }: Props) {
  return (
    <div className="periodo-selector">
      {([['dia','Hoy'],['semana','Semana'],['mes','Mes']] as [Periodo,string][]).map(([k,l]) => (
        <button key={k} className={`periodo-btn${value===k?' active':''}`} onClick={() => onChange(k)}>
          {l}
        </button>
      ))}
    </div>
  )
}
