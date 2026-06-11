import { LayoutDashboard, Package, Users, Receipt, Settings, LogOut, FileText, ShieldCheck, Sun, Moon, Coffee } from "lucide-react";
import { useEffect, useState } from "react";

interface Props {
  activeTab: string;
  onChangeTab: (tab: string) => void;
  operatorName: string;
  onLogout: () => void;
}

export function Sidebar({ activeTab, onChangeTab, operatorName, onLogout }: Props) {
  const getInitials = (name: string) => {
    return name
      .split(/[\s@]/)
      .filter(Boolean)
      .map((n) => n[0])
      .slice(0, 2)
      .join("")
      .toUpperCase();
  };

  const [isDark, setIsDark] = useState(() => document.documentElement.classList.contains('dark'));

  useEffect(() => {
    if (isDark) {
      document.documentElement.classList.add('dark');
      localStorage.setItem('theme', 'dark');
    } else {
      document.documentElement.classList.remove('dark');
      localStorage.setItem('theme', 'light');
    }
  }, [isDark]);

  const navItems = [
    { id: "arqueo",        label: "Arqueo de Caja",icon: <ShieldCheck size={18} /> },
    { id: "dashboard",     label: "Dashboard",     icon: <LayoutDashboard size={18} /> },
    { id: "productos",     label: "Productos",     icon: <Package size={18} /> },
    { id: "transacciones", label: "Transacciones", icon: <Receipt size={18} /> },
    { id: "reportes",      label: "Reportes Caja", icon: <FileText size={18} /> },
    { id: "operadores",    label: "Operadores",    icon: <Users size={18} /> },
    { id: "configuracion", label: "Configuración", icon: <Settings size={18} /> }
  ];

  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <div className="brand-icon-container" style={{ backgroundColor: '#f97316' }}>
          <Coffee size={24} strokeWidth={2.5} color="#ffffff" />
        </div>
        <div>
          <div className="sidebar-brand-name">Café de Barrio</div>
          <span className="sidebar-pos">Punto de Venta POS</span>
        </div>
      </div>

      <div className="profile-block">
        <div className="profile-avatar">{getInitials(operatorName)}</div>
        <div className="profile-info">
          <span className="profile-name">{operatorName}</span>
          <span className="profile-role">Administrador</span>
        </div>
      </div>

      <nav className="sidebar-nav">
        {navItems.map((item) => (
          <button
            key={item.id}
            onClick={() => onChangeTab(item.id)}
            className={`nav-link ${activeTab === item.id ? "active" : ""}`}
          >
            {item.icon}
            <span>{item.label}</span>
          </button>
        ))}
      </nav>


      <div className="sidebar-footer" style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
        <button 
          className="btn-logout" 
          onClick={() => setIsDark(!isDark)}
          style={{ justifyContent: 'center' }}
        >
          {isDark ? <Sun size={16} /> : <Moon size={16} />}
          <span>{isDark ? 'Modo Claro' : 'Modo Oscuro'}</span>
        </button>
        <button className="btn-logout" onClick={onLogout}>
          <LogOut size={16} />
          <span>Cerrar Sesión</span>
        </button>
      </div>
    </aside>
  );
}
