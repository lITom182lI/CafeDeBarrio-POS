import { useState, useEffect, useMemo, startTransition } from 'react'
import type {
  ProductoDto, CategoriaDto, MetodoPagoDto,
  CartItem, ComprobanteData, CreateTransaccionRequest, OperadorSession, TicketData,
} from '../types'
import { getProductos, getCategorias, getMetodosPago, getOperadores, crearTransaccion, OfflineError, getTurnoActivo } from '../api'
import { calcularTotales, generateLocalId } from '../utils'
import { config } from '../config'
import { useSync, savePending, setSimulatedOnline, getSimulatedOnline } from '../offline'
import {
  saveCatalogProductos, saveCatalogCategorias, saveCatalogMetodosPago,
  getCatalogProductos, getCatalogCategorias, getCatalogMetodosPago,
  saveCatalogOperadores, getCatalogOperadores
} from '../offline'
import ComprobanteModal from './ComprobanteModal'
import TicketModal from './TicketModal'
import ArqueoCierreModal from './ArqueoCierreModal'
import { Coffee } from 'lucide-react'

// List of Sub-Views
import TerminalVentasView from './TerminalVentasView'

// Side Nav Icons
import { Laptop, LogOut, ChevronRight, Wifi, WifiOff, Moon, Sun } from 'lucide-react'

interface Props {
  session: OperadorSession | null
  onLogout: () => void
}

type TabType = 'terminal' | 'turnos' | 'inventario' | 'historial' | 'reportes' | 'operadores'

export default function SalesModule({ session, onLogout }: Props) {
  // ── Tab state ─────────────────────────────────────────────────────────────
  const [activeTab, setActiveTab] = useState<TabType>('terminal')
  
  // ── Theme state ───────────────────────────────────────────────────────────
  const [isDarkMode, setIsDarkMode] = useState(() => document.documentElement.classList.contains("dark"))

  const toggleDark = () => {
    if (isDarkMode) {
      document.documentElement.classList.remove("dark")
      localStorage.setItem('theme', 'light')
      setIsDarkMode(false)
    } else {
      document.documentElement.classList.add("dark")
      localStorage.setItem('theme', 'dark')
      setIsDarkMode(true)
    }
  }

  // ── Catálogo ──────────────────────────────────────────────────────────────
  const [productos, setProductos] = useState<ProductoDto[]>([])
  const [categorias, setCategorias] = useState<CategoriaDto[]>([])
  const [metodosPago, setMetodosPago] = useState<MetodoPagoDto[]>([])

  // ── Carrito ───────────────────────────────────────────────────────────────
  const [cart, setCart] = useState<CartItem[]>([])

  // ── Pago ──────────────────────────────────────────────────────────────────
  const [metodoPagoId, setMetodoPagoId] = useState<number | null>(null)
  const [pagoDoble, setPagoDoble] = useState(false)
  const [metodoPago2Id, setMetodoPago2Id] = useState<number | null>(null)
  const [montoMetodo1, setMontoMetodo1] = useState('')
  const [montoEfectivo, setMontoEfectivo] = useState('')

  // ── Comprobante ───────────────────────────────────────────────────────────
  const [wantsComprobante, setWantsComprobante] = useState(false)
  const [showComprobanteModal, setShowComprobanteModal] = useState(false)
  const [comprobante, setComprobante] = useState<ComprobanteData | null>(null)

  // ── Checkout ──────────────────────────────────────────────────────────────
  const [processing, setProcessing] = useState(false)
  const [ticket, setTicket] = useState<TicketData | null>(null)
  const [errorMsg, setErrorMsg] = useState<string | null>(null)
  const [showCierreModal, setShowCierreModal] = useState(false)

  // ── Sync & Line monitoring ────────────────────────────────────────────────
  const { pendingCount, isOnline, refreshCount } = useSync()

  // ── Catalog loading trigger ───────────────────────────────────────────────
  async function loadCatalog() {
    try {
      const [prods, cats, metodos, ops] = await Promise.all([
        getProductos(),
        getCategorias(),
        getMetodosPago(),
        getOperadores()
      ])
      
      setProductos(prods.filter(p => p.activo))
      setCategorias(cats)
      setMetodosPago(metodos)
      
      if (metodos.length > 0) setMetodoPagoId(metodos[0].metodoPagoId)
      
      void saveCatalogProductos(prods)
      void saveCatalogCategorias(cats)
      void saveCatalogMetodosPago(metodos)
      void saveCatalogOperadores(ops)
    } catch (err) {
      if (err instanceof OfflineError) {
        const [prods, cats, metodos] = await Promise.all([
          getCatalogProductos(),
          getCatalogCategorias(),
          getCatalogMetodosPago(),
          getCatalogOperadores()
        ])
        if (prods.length > 0) {
          setProductos(prods.filter(p => p.activo))
          setCategorias(cats)
          setMetodosPago(metodos)
          if (metodos.length > 0) setMetodoPagoId(metodos[0].metodoPagoId)
        }
      }
    }
  }

  useEffect(() => { startTransition(() => { void loadCatalog() }) }, [])

  // Polling para detectar Cierre Forzado desde el Dashboard
  useEffect(() => {
    const checkInterval = setInterval(async () => {
      try {
        const turno = await getTurnoActivo()
        // Si la API responde exitosamente pero no hay turno (null), el administrador forzó el cierre
        if (!turno) {
          alert("Su sesión ha finalizado automáticamente debido a un Cierre Forzado de caja desde el Dashboard.");
          onLogout()
        }
      } catch (err) {
        // Si hay error de red o similar, simplemente ignoramos y seguimos intentando después
      }
    }, 10000) // Verificar cada 10 segundos

    return () => clearInterval(checkInterval)
  }, [onLogout])

  // ── Carrito Operations ────────────────────────────────────────────────────
  function addToCart(p: ProductoDto) {
    setCart(prev => {
      const existing = prev.find(i => i.productoId === p.productoId)
      if (existing) {
        if (p.seguimientoInventario && existing.cantidad >= p.cantidadDisponible) return prev
        return prev.map(i =>
          i.productoId === p.productoId ? { ...i, cantidad: i.cantidad + 1 } : i
        )
      }
      return [...prev, { productoId: p.productoId, nombre: p.nombre, precio: p.precio, cantidad: 1 }]
    })
  }

  function updateQty(productoId: number, delta: number) {
    const prod = productos.find(p => p.productoId === productoId)
    setCart(prev =>
      prev
        .map(i => {
          if (i.productoId === productoId) {
            const nextQty = i.cantidad + delta
            if (prod?.seguimientoInventario && nextQty > prod.cantidadDisponible) {
              return i // clamp to stock limits
            }
            return { ...i, cantidad: nextQty }
          }
          return i
        })
        .filter(i => i.cantidad > 0)
    )
  }

  function removeFromCart(productoId: number) {
    setCart(prev => prev.filter(i => i.productoId !== productoId))
  }

  function clearCart() {
    setCart([])
  }

  const { subtotal, igv, total } = useMemo(() => calcularTotales(cart), [cart])

  // ── Process payment sequence ──────────────────────────────────────────────
  async function handleCobrar() {
    setProcessing(true)
    setErrorMsg(null)

    const fechaHora = new Date().toISOString()
    const request: CreateTransaccionRequest = {
      sedeId: config.sedeId,
      clienteId: null,
      metodoPagoId: metodoPagoId!,
      items: cart.map(i => ({ productoId: i.productoId, cantidad: i.cantidad })),
      operadorId: session?.operadorId ?? null,
      tipoDocumento: comprobante?.tipoDocumento ?? null,
      numeroDocumento: comprobante?.numeroDocumento ?? null,
      razonSocial: comprobante ? comprobante.razonSocial : 'Público General',
      metodoPagoSecundarioId: pagoDoble ? metodoPago2Id : null,
      montoMetodoPrimario: pagoDoble ? parseFloat(montoMetodo1) : null,
    }

    let offline = false
    let transaccionId: number | undefined
    let localId: string | undefined

    const sinToken = !session?.token

    try {
      if (sinToken) throw new OfflineError()
      transaccionId = await crearTransaccion(request)
      
      // Update local product catalog state to correspond visually immediately!
      setProductos(prevProds => {
        return prevProds.map(p => {
          const cartItem = cart.find(i => i.productoId === p.productoId)
          if (cartItem && p.seguimientoInventario) {
            return {
              ...p,
              cantidadDisponible: Math.max(0, p.cantidadDisponible - cartItem.cantidad)
            }
          }
          return p
        })
      })
    } catch (err) {
      if (err instanceof OfflineError || sinToken) {
        offline = true
        localId = generateLocalId()
        
        // Save in DB Pending stream
        const metodoPagoNombre = metodosPago.find(m => m.metodoPagoId === metodoPagoId)?.nombre ?? 'Efectivo'
        await savePending({
          localId,
          sedeId: config.sedeId,
          metodoPagoId: metodoPagoId!,
          metodoPagoNombre,
          operadorId: session?.operadorId ?? null,
          items: request.items,
          cartSnapshot: [...cart],
          subtotal,
          igv,
          total,
          fechaLocal: fechaHora,
          tipoDocumento: comprobante?.tipoDocumento ?? null,
          numeroDocumento: comprobante?.numeroDocumento ?? null,
          razonSocial: (comprobante ? comprobante.razonSocial : 'Público General') ?? null,
          sincronizada: 0,
          transaccionIdServidor: null,
          error: null,
        })
        
        // Also update local visual catalog stock for seamless offline feeling
        setProductos(prevProds => {
          return prevProds.map(p => {
            const cartItem = cart.find(i => i.productoId === p.productoId)
            if (cartItem && p.seguimientoInventario) {
              return {
                ...p,
                cantidadDisponible: Math.max(0, p.cantidadDisponible - cartItem.cantidad)
              }
            }
            return p
          })
        })

        await refreshCount()
      } else {
        const errorText = err instanceof Error ? err.message : 'Error al registrar la comanda';
        if (errorText.includes('Producto.StockInsuficiente')) {
          // Extraer el mensaje amigable de stock
          setErrorMsg('No hay stock suficiente para uno de los productos seleccionados. Reduzca la cantidad en el pedido e intente de nuevo.');
        } else {
          setErrorMsg(`Error: ${errorText}`);
        }
        setProcessing(false)
        return
      }
    }

    setTicket({
      transaccionId,
      localId,
      items: [...cart],
      subtotal,
      igv,
      total,
      metodoPagoNombre: metodosPago.find(m => m.metodoPagoId === metodoPagoId)?.nombre ?? 'Efectivo',
      comprobante,
      fechaHora,
      offline,
      metodoPagoSecundarioNombre: pagoDoble && metodoPago2Id
        ? (metodosPago.find(m => m.metodoPagoId === metodoPago2Id)?.nombre ?? undefined)
        : undefined,
      montoMetodoPrimario: pagoDoble ? parseFloat(montoMetodo1) : undefined
    })

    // Reset shopping cart state
    setCart([])
    setPagoDoble(false)
    setMetodoPago2Id(null)
    setMontoMetodo1('')
    setMontoEfectivo('')
    setWantsComprobante(false)
    setComprobante(null)
    setProcessing(false)
  }

  // Toggle online simulator channel dynamically
  const toggleNetworkConnection = () => {
    const currentOn = getSimulatedOnline()
    setSimulatedOnline(!currentOn)
  }

  // Left sidebar menu items specification matching exact elements
  const sidebarItems = [
    { type: 'terminal', label: 'TERMINAL DE VENTAS', icon: <Laptop size={18} /> },
  ] as const

  return (
    <div className={`h-screen flex bg-[#F8FAFC] dark:bg-gray-900 text-[#334155] dark:text-gray-100 overflow-hidden font-sans select-none`}>
      
      {/* ── LEFT NAVIGATION SIDEBAR ── */}
      <aside className="w-64 bg-white dark:bg-gray-800 dark:border-gray-700 border-r border-[#E2E8F0] flex flex-col justify-between flex-shrink-0 transition-colors">
        
        {/* Brand identifier - Match Screenshot Header */}
        <div className="pt-8 border-b border-[#E2E8F0] dark:border-gray-700">
          <div className="px-6 flex items-center gap-3">
            <div className="bg-[#f97316] w-[34px] h-[34px] rounded-lg text-white flex items-center justify-center transition-colors">
              <Coffee size={20} strokeWidth={2.5} />
            </div>
            <div>
              <h2 className="text-[#7C2D12] dark:text-orange-500 font-extrabold text-[1.15rem] tracking-[-0.03em] leading-[1.1] transition-colors">Café de Barrio</h2>
              <p className="text-[#7C2D12] dark:text-orange-500/80 text-[0.65rem] uppercase font-bold tracking-[0.08em] mt-[2px] transition-colors">Punto de Venta POS</p>
            </div>
          </div>

          {/* Cajero / Operador profile badge - Dashboard exact match */}
          <div className="px-6 py-5 flex items-center gap-3 border-t border-[#E2E8F0] dark:border-gray-700 bg-transparent transition-colors mt-6">
            <div className="w-[38px] h-[38px] min-w-[38px] shrink-0 rounded-full bg-white dark:bg-gray-800 text-[#7C2D12] dark:text-orange-500 flex items-center justify-center font-extrabold text-[0.9rem] border-[1.5px] border-[#E2E8F0] dark:border-gray-700 transition-colors">
              {session?.nombre ? session.nombre.substring(0, 2).toUpperCase() : 'CG'}
            </div>
            <div className="min-w-0 flex flex-col">
              <p className="font-extrabold text-[#1E293B] dark:text-gray-100 text-[0.88rem] tracking-[-0.01em] truncate transition-colors">
                {session?.nombre || 'Cajero Genérico'}
              </p>
              <p className="text-[#64748B] dark:text-gray-400 text-[0.72rem] font-semibold tracking-[0.02em] transition-colors">
                Cajero POS
              </p>
            </div>
          </div>
        </div>

        {/* Sidebar Nav anchors list - selected matches Elegant Clay bg exactly */}
        <nav className="flex-1 px-3 py-6 space-y-1 overflow-y-auto">
          {sidebarItems.map(item => {
            const isSelected = activeTab === item.type
            return (
              <button
                key={item.type}
                onClick={() => setActiveTab(item.type)}
                className={`w-full px-3.5 py-3 rounded-xl flex items-center gap-3 font-bold text-xs transition duration-150 active:scale-95 text-left cursor-pointer ${
                  isSelected
                    ? 'bg-[#7C2D12] text-white shadow-sm font-bold shadow-[#7C2D12]/10'
                    : 'text-[#334155]/70 dark:text-gray-300 hover:bg-[#F8FAFC] dark:hover:bg-gray-700 hover:text-[#1E293B] dark:hover:text-white border border-transparent'
                }`}
              >
                <span className={isSelected ? 'text-white' : 'text-[#334155]/60 dark:text-gray-400'}>
                  {item.icon}
                </span>
                <span className="flex-1">{item.label}</span>
                {isSelected && <ChevronRight size={14} className="text-white/80" />}
              </button>
            )
          })}
        </nav>

        {/* Offline Toggle and Log out stack */}
        <div className="p-3 border-t border-[#E2E8F0] dark:border-gray-700 space-y-2 flex-shrink-0 bg-white dark:bg-gray-800 transition-colors">
          
          <button
            onClick={toggleDark}
            className="w-full py-2.5 px-3 rounded-xl flex items-center justify-center transition-colors text-[11px] border cursor-pointer font-bold bg-orange-50 dark:bg-orange-900/30 text-[#7C2D12] dark:text-orange-400 border-orange-200 dark:border-orange-800/50 hover:bg-orange-100 dark:hover:bg-orange-900/50"
          >
            <span className="flex items-center gap-1.5 uppercase tracking-wide">
              {isDarkMode ? <Sun size={14} /> : <Moon size={14} />}
              <span>{isDarkMode ? 'Modo Claro' : 'Modo Oscuro'}</span>
            </span>
          </button>

          {/* Real network simulation block - Clicking this lets user toggle network state! */}
          <button
            onClick={toggleNetworkConnection}
            className={`w-full py-2.5 px-3 rounded-xl flex items-center justify-between transition-colors text-left text-[11px] border cursor-pointer font-bold ${
              isOnline
                ? 'bg-emerald-50/50 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 border-emerald-200 dark:border-emerald-800 hover:bg-emerald-100/50 dark:hover:bg-emerald-900/50'
                : 'bg-rose-50 dark:bg-rose-900/30 text-rose-700 dark:text-rose-400 border-rose-200 dark:border-rose-800 hover:bg-rose-100 dark:hover:bg-rose-900/50 animate-pulse'
            }`}
          >
            <span className="flex items-center gap-1.5 uppercase tracking-wide">
              {isOnline ? <Wifi size={14} /> : <WifiOff size={14} />}
              <span>{isOnline ? '● En Línea' : '○ Desconectado'}</span>
            </span>
            <span className="text-[9px] bg-[#7C2D12]/10 dark:bg-[#7C2D12]/30 text-[#7C2D12] dark:text-orange-300 px-1.5 py-0.5 rounded font-mono font-bold">Simular</span>
          </button>

          {pendingCount > 0 && (
            <div className="bg-[#7C2D12]/15 p-2.5 rounded-xl text-center text-xs animate-bounce border border-[#7C2D12]/20">
              <span className="text-[#7C2D12] font-bold block text-[10px] uppercase tracking-wide animate-pulse">
                ⚠️ {pendingCount} Comanda{pendingCount > 1 ? 's' : ''} pendiente{pendingCount > 1 ? 's' : ''}
              </span>
            </div>
          )}

          <button
            onClick={() => setShowCierreModal(true)}
            className="w-full py-2.5 text-[#475569] dark:text-gray-300 bg-[#F8FAFC] dark:bg-gray-700 rounded-xl border border-[#E2E8F0] dark:border-gray-600 hover:border-red-200 dark:hover:border-red-800 hover:text-red-600 dark:hover:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/30 font-bold text-xs tracking-wide flex items-center justify-center gap-2 transition active:scale-95 cursor-pointer"
          >
            <LogOut size={14} />
            <span>Cerrar Sesión</span>
          </button>
        </div>

      </aside>

      {/* ── RIGHT MAIN PANEL SUB-VIEWS MOUNT POINT ── */}
      <main className="flex-1 flex flex-col overflow-hidden h-full bg-[#F8FAFC] dark:bg-gray-900 transition-colors">
        <TerminalVentasView
          productos={productos}
          categorias={categorias}
          metodosPago={metodosPago}
          cart={cart}
          addToCart={addToCart}
          updateQty={updateQty}
          removeFromCart={removeFromCart}
          clearCart={clearCart}
          total={total}
          subtotal={subtotal}
          igv={igv}
          metodoPagoId={metodoPagoId}
          setMetodoPagoId={setMetodoPagoId}
          pagoDoble={pagoDoble}
          setPagoDoble={setPagoDoble}
          metodoPago2Id={metodoPago2Id}
          setMetodoPago2Id={setMetodoPago2Id}
          montoMetodo1={montoMetodo1}
          setMontoMetodo1={setMontoMetodo1}
          montoEfectivo={montoEfectivo}
          setMontoEfectivo={setMontoEfectivo}
          wantsComprobante={wantsComprobante}
          setWantsComprobante={setWantsComprobante}
          comprobante={comprobante}
          setComprobante={setComprobante}
          setShowComprobanteModal={setShowComprobanteModal}
          processing={processing}
          handleCobrar={handleCobrar}
          errorMsg={errorMsg}
        />
      </main>

      {/* ── GLOBAL SCREEN MODAL COMPONENTS ── */}
      {showComprobanteModal && (
        <ComprobanteModal
          onConfirm={data => {
            setComprobante(data)
            setShowComprobanteModal(false)
            setWantsComprobante(true)
          }}
          onCancel={() => {
            setShowComprobanteModal(false)
            if (!comprobante) setWantsComprobante(false)
          }}
        />
      )}

      {ticket && (
        <TicketModal ticket={ticket} onClose={() => setTicket(null)} />
      )}

      {showCierreModal && session && (
        <ArqueoCierreModal
          session={session}
          onClose={() => setShowCierreModal(false)}
          onSuccess={() => {
            setShowCierreModal(false)
            onLogout()
          }}
        />
      )}

    </div>
  )
}
