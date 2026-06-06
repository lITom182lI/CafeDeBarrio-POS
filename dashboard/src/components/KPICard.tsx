interface Props { label: string; value: string; icon: string; sub?: string; }

export function KPICard({ label, value, icon, sub }: Props) {
  return (
    <div className="card">
      <div className="kpi-top">
        <span className="kpi-label">{label}</span>
        <span className="kpi-icon">{icon}</span>
      </div>
      <div className="kpi-value">{value}</div>
      {sub && <div className="kpi-sub">{sub}</div>}
    </div>
  )
}
