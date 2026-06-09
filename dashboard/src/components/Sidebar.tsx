import { LayoutDashboard, Package, Users, Receipt, Settings, LogOut } from "lucide-react";

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

  const navItems = [
    { id: "dashboard",     label: "Dashboard",     icon: <LayoutDashboard size={18} /> },
    { id: "productos",     label: "Productos",     icon: <Package size={18} /> },
    { id: "transacciones", label: "Transacciones", icon: <Receipt size={18} /> },
    { id: "operadores",    label: "Operadores",    icon: <Users size={18} /> },
    { id: "configuracion", label: "Configuración", icon: <Settings size={18} /> }
  ];

  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <div className="brand-icon-container">☕</div>
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

      <div className="sidebar-footer">
        <button className="btn-logout" onClick={onLogout}>
          <LogOut size={16} />
          <span>Cerrar Sesión</span>
        </button>
      </div>
    </aside>
  );
}
