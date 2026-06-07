import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthProvider'
import { useAuth } from './hooks/useAuth'
import { Sidebar } from './components/Sidebar'
import { LoginPage } from './pages/LoginPage'
import { DashboardPage } from './pages/DashboardPage'
import { ProductosPage } from './pages/ProductosPage'
import { TransaccionesPage } from './pages/TransaccionesPage'
import { ConfiguracionPage } from './pages/ConfiguracionPage'
import { OperadoresPage } from './pages/OperadoresPage'

function AppShell() {
  const { token } = useAuth()
  if (!token) return <Navigate to="/login" replace />
  return (
    <div className="app-layout">
      <Sidebar />
      <main className="main-content">
        <Routes>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<DashboardPage />} />
          <Route path="productos" element={<ProductosPage />} />
          <Route path="transacciones" element={<TransaccionesPage />} />
          <Route path="configuracion" element={<ConfiguracionPage />} />
          <Route path="operadores" element={<OperadoresPage />} />
        </Routes>
      </main>
    </div>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/*" element={<AppShell />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
