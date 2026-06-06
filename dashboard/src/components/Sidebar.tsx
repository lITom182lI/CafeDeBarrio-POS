import { NavLink } from 'react-router-dom'

const links = [
  { to: '/dashboard',      icon: '📊', label: 'Dashboard' },
  { to: '/productos',      icon: '📦', label: 'Productos' },
  { to: '/transacciones',  icon: '💳', label: 'Transacciones' },
]

export function Sidebar() {
  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <span className="sidebar-brand">POS System</span>
      </div>
      <nav className="sidebar-nav">
        {links.map(l => (
          <NavLink key={l.to} to={l.to}
            className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}>
            <span>{l.icon}</span>
            <span>{l.label}</span>
          </NavLink>
        ))}
      </nav>
      <div className="sidebar-footer">
        <button className="nav-link btn-logout">
          ↩ Logout
        </button>
      </div>
    </aside>
  )
}
