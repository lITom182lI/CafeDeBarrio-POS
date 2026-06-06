import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const links = [
  { to: '/dashboard',     icon: '📊', label: 'Dashboard' },
  { to: '/productos',     icon: '📦', label: 'Productos' },
  { to: '/transacciones', icon: '💳', label: 'Transacciones' },
  { to: '/operadores',    icon: '👤', label: 'Operadores' },
  { to: '/configuracion', icon: '⚙', label: 'Configuracion' },
]

export function Sidebar() {
  const { logout } = useAuth()
  const navigate   = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

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
        <button className="nav-link btn-logout" onClick={handleLogout}>
          ↩ Logout
        </button>
      </div>
    </aside>
  )
}
