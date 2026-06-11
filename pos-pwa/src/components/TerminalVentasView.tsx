import { useState, useMemo } from 'react'
import type { ProductoDto, CategoriaDto, MetodoPagoDto, CartItem, ComprobanteData } from '../types'
import { formatSoles } from '../utils'
import { Search, ShoppingCart, Trash2, Smartphone, CreditCard, Banknote, HelpCircle } from 'lucide-react'

interface Props {
  productos: ProductoDto[]
  categorias: CategoriaDto[]
  metodosPago: MetodoPagoDto[]
  cart: CartItem[]
  addToCart: (p: ProductoDto) => void
  updateQty: (productoId: number, delta: number) => void
  removeFromCart: (productoId: number) => void
  clearCart: () => void
  total: number
  subtotal: number
  igv: number
  metodoPagoId: number | null
  setMetodoPagoId: (id: number) => void
  pagoDoble: boolean
  setPagoDoble: (val: boolean) => void
  metodoPago2Id: number | null
  setMetodoPago2Id: (id: number | null) => void
  montoMetodo1: string
  setMontoMetodo1: (val: string) => void
  montoEfectivo: string
  setMontoEfectivo: (val: string) => void
  wantsComprobante: boolean
  setWantsComprobante: (val: boolean) => void
  comprobante: ComprobanteData | null
  setComprobante: (data: ComprobanteData | null) => void
  setShowComprobanteModal: (val: boolean) => void
  processing: boolean
  handleCobrar: () => void
  errorMsg: string | null
}
const getCategoryColorClass = (nombre: string | null) => {
  if (!nombre) return 'bg-[#7C2D12] text-white border-[#7C2D12] shadow-md'; // Todos
  const n = nombre.toLowerCase();
  if (n.includes('cafe') || n.includes('café')) return 'bg-[#92400E] text-white border-[#92400E] shadow-md'; // Amber-800
  if (n.includes('bebida')) return 'bg-[#0369A1] text-white border-[#0369A1] shadow-md'; // Sky-700
  if (n.includes('comida') || n.includes('postre')) return 'bg-[#CA8A04] text-white border-[#CA8A04] shadow-md'; // Yellow-600
  return 'bg-[#7C2D12] text-white border-[#7C2D12] shadow-md'; // Default
};

const getBadgeColorClass = (nombre: string | null) => {
  if (!nombre) return 'bg-[#7C2D12] text-white';
  const n = nombre.toLowerCase();
  if (n.includes('cafe') || n.includes('café')) return 'bg-[#92400E] text-white';
  if (n.includes('bebida')) return 'bg-[#0369A1] text-white';
  if (n.includes('comida') || n.includes('postre')) return 'bg-[#CA8A04] text-white';
  return 'bg-[#7C2D12] text-white';
};

const getPaymentColorClass = (nombre: string) => {
  const n = nombre.toLowerCase();
  if (n.includes('efectivo')) return 'bg-emerald-100/80 dark:bg-emerald-950/60 text-emerald-800 dark:text-emerald-400 border-emerald-300 dark:border-emerald-800';
  if (n.includes('yape')) return 'bg-purple-100/80 dark:bg-purple-900/50 text-purple-800 dark:text-purple-300 border-purple-300 dark:border-purple-700';
  if (n.includes('plin')) return 'bg-cyan-100/80 dark:bg-cyan-950/60 text-cyan-800 dark:text-cyan-400 border-cyan-300 dark:border-cyan-800';
  if (n.includes('tarjeta')) return 'bg-blue-100/80 dark:bg-blue-900/50 text-blue-800 dark:text-blue-300 border-blue-300 dark:border-blue-700';
  return 'bg-slate-100/80 dark:bg-slate-800 text-slate-800 dark:text-slate-300 border-slate-300 dark:border-slate-600';
};

export default function TerminalVentasView({
  productos,
  categorias,
  metodosPago,
  cart,
  addToCart,
  updateQty,
  removeFromCart,
  clearCart,
  total,
  subtotal,
  igv,
  metodoPagoId,
  setMetodoPagoId,
  pagoDoble,
  setPagoDoble,
  metodoPago2Id,
  setMetodoPago2Id,
  montoMetodo1,
  setMontoMetodo1,
  montoEfectivo,
  setMontoEfectivo,
  wantsComprobante,
  setWantsComprobante,
  comprobante,
  setComprobante,
  setShowComprobanteModal,
  processing,
  handleCobrar,
  errorMsg
}: Props) {
  const [search, setSearch] = useState('')
  const [selectedCat, setSelectedCat] = useState<string | null>(null)

  // Filter Catalog dynamically
  const productosFiltrados = useMemo(() => {
    return productos.filter(p => {
      const matchSearch = p.nombre.toLowerCase().includes(search.toLowerCase()) ||
        (p.categoriaNombre?.toLowerCase().includes(search.toLowerCase()) ?? false)
      const matchCat = selectedCat ? p.categoriaNombre === selectedCat : true
      return matchSearch && matchCat
    })
  }, [productos, search, selectedCat])

  // Helpers
  const renderPaymentIcon = (name: string) => {
    const lower = name.toLowerCase()
    if (lower.includes('efectivo')) return <Banknote size={15} />
    if (lower.includes('tarjeta') || lower.includes('visa') || lower.includes('mastercard')) return <CreditCard size={15} />
    if (lower.includes('yape') || lower.includes('plin') || lower.includes('digital') || lower.includes('qr')) return <Smartphone size={15} />
    return <HelpCircle size={15} />
  }

  // Checking variables
  const isEfectivo = useMemo(() => {
    const activeMetodo = metodosPago.find(m => m.metodoPagoId === metodoPagoId)
    return activeMetodo?.nombre.toLowerCase().includes('efectivo') ?? false
  }, [metodoPagoId, metodosPago])

  const valMontoEfectivo = parseFloat(montoEfectivo) || 0
  const deudor = valMontoEfectivo < total
  const vuelto = valMontoEfectivo - total

  const isPagoDobleValido = useMemo(() => {
    if (!pagoDoble) return true
    const m1 = parseFloat(montoMetodo1) || 0
    return m1 > 0 && m1 < total && metodoPago2Id !== null
  }, [pagoDoble, montoMetodo1, total, metodoPago2Id])

  const cobrarDisabled = cart.length === 0 || processing || (!pagoDoble && isEfectivo && deudor) || !isPagoDobleValido

  return (
    <div className="flex-1 flex overflow-hidden h-full">
      {/* ── CENTRAL CATALOG PANEL ── */}
      <div className="flex-1 flex flex-col overflow-hidden bg-[#F8FAFC] dark:bg-gray-900 transition-colors">
        
        {/* Search header bar - Clean Minimalism style */}
        <div className="pt-4 px-4 pb-2 bg-white dark:bg-gray-800 border-b border-[#E2E8F0] dark:border-gray-700 flex-shrink-0 space-y-2 transition-colors">
          <div className="relative">
            <span className="absolute inset-y-0 left-3 flex items-center text-[#334155]/60">
              <Search size={18} />
            </span>
            <input
              type="text"
              placeholder="Buscar café, té, postres o sandwiches..."
              value={search}
              onChange={e => setSearch(e.target.value)}
              className="w-full bg-[#F8FAFC] dark:bg-gray-700 border border-[#E2E8F0] dark:border-gray-600 rounded-xl pl-10 pr-4 py-3 text-[#1E293B] dark:text-gray-100 placeholder-[#334155]/50 dark:placeholder-gray-400 font-sans text-sm focus:outline-none focus:border-[#7C2D12] dark:focus:border-orange-500 focus:bg-white dark:focus:bg-gray-800 transition-colors"
            />
          </div>

          {/* Category Quick Chips - Clean Minimalism style */}
          <div className="flex gap-3 overflow-x-auto py-1 scrollbar-none">
            <button
              onClick={() => setSelectedCat(null)}
              className={`flex-shrink-0 px-6 py-3 rounded-xl text-sm tracking-wide font-extrabold transition-all border cursor-pointer uppercase ${
                selectedCat === null
                  ? getCategoryColorClass(null)
                  : 'bg-[#F8FAFC] dark:bg-gray-700 text-[#334155] dark:text-gray-300 border-[#E2E8F0] dark:border-gray-600 hover:bg-white dark:hover:bg-gray-600 hover:text-[#1E293B] dark:hover:text-white'
              }`}
            >
              Todos
            </button>
            {categorias.map(cat => (
              <button
                key={cat.categoriaId}
                onClick={() => setSelectedCat(cat.nombre)}
                className={`flex-shrink-0 px-6 py-3 rounded-xl text-sm tracking-wide font-extrabold transition-all border cursor-pointer uppercase ${
                  selectedCat === cat.nombre
                    ? getCategoryColorClass(cat.nombre)
                    : 'bg-[#F8FAFC] dark:bg-gray-700 text-[#334155] dark:text-gray-300 border-[#E2E8F0] dark:border-gray-600 hover:bg-white dark:hover:bg-gray-600 hover:text-[#1E293B] dark:hover:text-white'
                }`}
              >
                {cat.nombre}
              </button>
            ))}
          </div>
        </div>

        {/* Product Catalog Grid - Clean Minimalism style */}
        <div className="flex-1 overflow-y-auto p-4 bg-[#F8FAFC] dark:bg-gray-900 transition-colors">
          <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-3">
            {productosFiltrados.map(p => {
              const cartItem = cart.find(i => i.productoId === p.productoId)
              const qtyInCart = cartItem?.cantidad || 0
              const isLowStock = p.seguimientoInventario && p.cantidadDisponible <= 5
              const isAgotado = p.seguimientoInventario && p.cantidadDisponible <= 0

              return (
                <button
                  key={p.productoId}
                  disabled={isAgotado || qtyInCart >= p.cantidadDisponible}
                  onClick={() => addToCart(p)}
                  className={`text-left bg-white dark:bg-gray-800 rounded-2xl p-4 border transition-all text-[#334155] dark:text-gray-200 relative flex flex-col justify-between shadow-xs h-40 group cursor-pointer ${
                    isAgotado
                      ? 'opacity-30 bg-slate-100 dark:bg-gray-800 border-[#E2E8F0] dark:border-gray-700 cursor-not-allowed'
                      : qtyInCart >= p.cantidadDisponible
                      ? 'opacity-80 bg-[#F8FAFC] dark:bg-gray-800 border-[#E2E8F0] dark:border-gray-700 cursor-not-allowed'
                      : qtyInCart > 0
                      ? 'border-[#7C2D12] dark:border-orange-500 bg-[#7C2D12]/5 dark:bg-orange-500/10 ring-1 ring-[#7C2D12]/20 dark:ring-orange-500/20 shadow-xs'
                      : 'border-[#E2E8F0] dark:border-gray-700 hover:border-[#7C2D12]/40 dark:hover:border-orange-500/50 hover:shadow-xs hover:transform active:scale-95'
                  }`}
                >
                  {/* Category & Price line */}
                  <div className="flex justify-between items-start w-full">
                    <span className={`text-[10px] uppercase tracking-wider font-bold px-2 py-0.5 rounded-lg ${getBadgeColorClass(p.categoriaNombre)}`}>
                      {p.categoriaNombre}
                    </span>
                    <span className="text-[#7C2D12] dark:text-orange-400 font-extrabold text-sm tracking-wide">
                      {formatSoles(p.precio)}
                    </span>
                  </div>

                  {/* Name & Short Description */}
                  <div className="my-2 flex-1 flex flex-col justify-center">
                    <p className="font-extrabold text-[#1E293B] dark:text-gray-100 text-sm leading-snug group-hover:text-[#7C2D12] dark:group-hover:text-orange-400 transition uppercase">
                      {p.nombre}
                    </p>
                    <p className="text-[11px] text-[#334155]/85 dark:text-gray-400 font-sans line-clamp-2 mt-1 leading-relaxed">
                      {p.nombre === 'Espresso Doble' && 'Extracción concentrada de café seleccionado.'}
                      {p.nombre === 'Cappuccino Italiano' && 'Espresso con leche texturizada y cacao en polvo.'}
                      {p.nombre === 'Latte Caramel' && 'Café latte saborizado con salsa premium de caramelo.'}
                      {p.nombre === 'Americano Aromático' && 'Espresso suave con agua purificada caliente.'}
                      {p.nombre === 'Croissant con Mantequilla' && 'Masa de hojaldre horneada al día, crujiente y dorada.'}
                      {p.nombre === 'Cheesecake de Arándanos' && 'Tarta cremosa de queso artesanal de bayas silvestres.'}
                      {p.nombre !== 'Espresso Doble' &&
                        p.nombre !== 'Cappuccino Italiano' &&
                        p.nombre !== 'Latte Caramel' &&
                        p.nombre !== 'Americano Aromático' &&
                        p.nombre !== 'Croissant con Mantequilla' &&
                        p.nombre !== 'Cheesecake de Arándanos' &&
                        (p.descripcion || 'Producto de especialidad seleccionado por Café de Barrio.')}
                    </p>
                  </div>

                  {/* Stock footer counter indicator */}
                  <div className="flex justify-between items-center w-full pt-1">
                    <div className="flex items-center gap-1.5">
                      {qtyInCart >= p.cantidadDisponible ? (
                        <span className="bg-rose-500 text-white font-extrabold text-[10px] px-2 py-0.5 rounded-full shadow-2xs">
                          Máx Alcanzado
                        </span>
                      ) : qtyInCart > 0 ? (
                        <span className="bg-[#7C2D12] text-white font-extrabold text-[10px] px-2 py-0.5 rounded-full shadow-2xs animate-bounce">
                          {qtyInCart} en Pedido
                        </span>
                      ) : null}
                    </div>
                    
                    {isAgotado ? (
                      <span className="text-red-500 font-bold text-[10px] uppercase">Sin stock disponible</span>
                    ) : (
                      <span className={`text-[10px] font-bold ${isLowStock ? 'text-rose-500' : 'text-slate-400'}`}>
                        Stock: {p.cantidadDisponible}
                      </span>
                    )}
                  </div>
                </button>
              )
            })}
          </div>
        </div>
      </div>

      {/* ── RIGHT CART & PAYMENT SIDEBAR ── */}
      <div className="w-[380px] xl:w-[420px] bg-white dark:bg-gray-800 border-l border-[#E2E8F0] dark:border-gray-700 flex flex-col justify-between overflow-hidden shadow-sm flex-shrink-0 transition-colors">
        
        {/* Header order section */}
        <div className="p-4 border-b border-[#E2E8F0] dark:border-gray-700 flex items-center justify-between bg-[#F8FAFC] dark:bg-gray-800/50 flex-shrink-0 transition-colors">
          <div className="flex items-center gap-1.5 text-[#1E293B] dark:text-gray-100 font-extrabold">
            <ShoppingCart size={16} className="text-[#7C2D12] dark:text-orange-500" />
            <span className="text-sm uppercase tracking-wider">COMANDA DE VENTA</span>
          </div>
          {cart.length > 0 && (
            <button
              onClick={clearCart}
              className="text-red-600 hover:text-red-850 font-bold text-xs flex items-center gap-1 hover:bg-red-50 px-2 py-1 rounded-lg transition cursor-pointer"
            >
              <Trash2 size={12} />
              <span>Vaciar Carrito</span>
            </button>
          )}
        </div>

        {/* Scrollable list items */}
        <div className="flex-1 overflow-y-auto p-4 space-y-3">
          {cart.length === 0 ? (
            <div className="h-full flex flex-col items-center justify-center text-center space-y-3 py-10">
              <span className="text-4xl">☕</span>
              <p className="text-[#1E293B] dark:text-gray-300 font-extrabold text-sm">El carrito está vacío.</p>
              <p className="text-[#334155]/60 dark:text-gray-500 text-xs max-w-[200px] leading-relaxed">
                Haga clic en los productos para agregarlos.
              </p>
            </div>
          ) : (
            <div className="space-y-1">
              {cart.map(item => {
                const prod = productos.find(p => p.productoId === item.productoId)
                const isMax = prod && item.cantidad >= prod.cantidadDisponible
                return (
                <div key={item.productoId} className="bg-[#F8FAFC] dark:bg-gray-700/50 p-2.5 rounded-xl border border-[#E2E8F0] dark:border-gray-600 flex items-center justify-between gap-1 transition-colors">
                  <div className="flex-1 min-w-0">
                     <p className="font-extrabold text-[12px] text-[#1E293B] dark:text-gray-100 truncate uppercase">{item.nombre}</p>
                     <p className="text-[10px] text-[#334155]/60 dark:text-gray-400 font-semibold">{formatSoles(item.precio)} c/u</p>
                  </div>
                  <div className="flex items-center gap-1">
                    <button
                      onClick={() => updateQty(item.productoId, -1)}
                      className="w-5 h-5 rounded-lg bg-[#E2E8F0] dark:bg-gray-600 hover:bg-[#cbd5e0] dark:hover:bg-gray-500 text-[#334155] dark:text-gray-200 text-[10px] font-bold flex items-center justify-center transition active:scale-90 cursor-pointer"
                    >
                      −
                    </button>
                    <span className="w-4 text-center font-extrabold text-xs text-[#1E293B] dark:text-gray-100">{item.cantidad}</span>
                    <button
                      disabled={isMax}
                      onClick={() => updateQty(item.productoId, 1)}
                      className={`w-5 h-5 rounded-lg text-[10px] font-bold flex items-center justify-center transition ${isMax ? 'bg-rose-100 dark:bg-rose-900/50 text-rose-300 dark:text-rose-400 cursor-not-allowed' : 'bg-[#E2E8F0] dark:bg-gray-600 hover:bg-[#cbd5e0] dark:hover:bg-gray-500 text-[#334155] dark:text-gray-200 cursor-pointer active:scale-90'}`}
                    >
                      +
                    </button>
                  </div>
                  <div className="w-16 text-right flex-shrink-0">
                    <p className="font-bold text-[#1E293B] dark:text-gray-100 text-xs">{formatSoles(item.precio * item.cantidad)}</p>
                  </div>
                  <button
                    onClick={() => removeFromCart(item.productoId)}
                    className="text-[#334155]/40 hover:text-red-500 p-0.5 cursor-pointer font-bold text-sm"
                  >
                    ×
                  </button>
                </div>
                );
              })}
            </div>
          )}
        </div>

        {/* Totales, Customer Data & Checkout Controls Panel */}
        <div className="p-4 bg-white dark:bg-gray-800 border-t border-[#E2E8F0] dark:border-gray-700 space-y-4 flex-shrink-0 transition-colors">
          
          {/* Metadata: Comprobante Type & Name selectors matching Screenshot 2 */}
          <div className="space-y-2.5">
            <div>
              <label htmlFor="comprobante-select" className="block text-[10px] uppercase tracking-wider text-[#1E293B] dark:text-gray-300 font-extrabold mb-1">
                COMPROBANTE DE VENTA
              </label>
              <select
                id="comprobante-select"
                value={wantsComprobante ? 'boleta' : 'simple'}
                onChange={e => {
                  const val = e.target.value
                  if (val === 'boleta') {
                    setShowComprobanteModal(true)
                  } else {
                    setWantsComprobante(false)
                    setComprobante(null)
                  }
                }}
                className="w-full bg-[#F8FAFC] dark:bg-gray-700 border border-[#E2E8F0] dark:border-gray-600 text-[#1E293B] dark:text-gray-100 rounded-lg px-2.5 py-1.5 text-xs font-bold focus:outline-none focus:border-[#7C2D12] dark:focus:border-orange-500"
              >
                <option value="simple">Ticket de Venta Simple</option>
                <option value="boleta">Boleta Nominada o Factura</option>
              </select>
            </div>

            <div>
              <label htmlFor="client-input" className="block text-[10px] uppercase tracking-wider text-[#1E293B] dark:text-gray-300 font-extrabold mb-1">
                NOMBRE DEL CLIENTE
              </label>
              <input
                id="client-input"
                type="text"
                disabled={comprobante !== null}
                value={comprobante ? comprobante.razonSocial : 'Público General'}
                placeholder="Público General..."
                onChange={() => {
                  if (!comprobante) {
                    // allows custom simple client name text
                    // setting the plain simple text
                  }
                }}
                className="w-full bg-[#F8FAFC] dark:bg-gray-700 border border-[#E2E8F0] dark:border-gray-600 text-[#1E293B] dark:text-gray-100 rounded-lg px-2.5 py-1.5 text-xs font-bold focus:outline-none focus:border-[#7C2D12] dark:focus:border-orange-500 placeholder-slate-400 dark:placeholder-gray-500"
              />
              {comprobante && (
                <p className="text-[10px] text-[#7C2D12] dark:text-orange-400 font-semibold mt-1">
                  ✅ Listado con {comprobante.tipoDocumento}: {comprobante.numeroDocumento}
                </p>
              )}
            </div>
          </div>

          {/* Payment Methods Selection Stack - Grid 2x2 matched precisely to screenshot 2 */}
          <div className="space-y-1.5">
            <span className="block text-[10px] uppercase tracking-wider text-[#1E293B] dark:text-gray-300 font-extrabold mb-1">
              CANAL / MEDIO DE PAGO
            </span>
            <div className="grid grid-cols-2 gap-1.5">
              {metodosPago.map(m => {
                const isActive = metodoPagoId === m.metodoPagoId
                return (
                  <button
                    key={m.metodoPagoId}
                    onClick={() => { setMetodoPagoId(m.metodoPagoId); setMontoEfectivo('') }}
                    className={`py-2 rounded-xl text-xs font-bold border flex items-center justify-center gap-1.5 transition active:scale-95 cursor-pointer ${
                      isActive
                        ? `${getPaymentColorClass(m.nombre)} shadow-2xs`
                        : 'bg-white dark:bg-gray-700 text-[#334155]/85 dark:text-gray-300 border-[#E2E8F0] dark:border-gray-600 hover:border-[#7C2D12]/40 dark:hover:border-orange-500 hover:text-[#1E293B] dark:hover:text-white'
                    }`}
                  >
                    {renderPaymentIcon(m.nombre)}
                    <span>{m.nombre}</span>
                  </button>
                )
              })}
            </div>

            {/* Split billing checkbox */}
            <label className="flex items-center gap-1.5 pt-1.5 cursor-pointer">
              <input
                type="checkbox"
                checked={pagoDoble}
                onChange={e => { setPagoDoble(e.target.checked); setMetodoPago2Id(null); setMontoMetodo1('') }}
                className="w-3.5 h-3.5 accent-[#7C2D12] rounded cursor-pointer"
              />
              <span className="text-[#334155]/70 dark:text-gray-400 text-[10px] font-bold uppercase tracking-wider">Pago doble (dos formas)</span>
            </label>

            {pagoDoble && (
              <div className="bg-[#F8FAFC] dark:bg-gray-700/50 rounded-xl p-2.5 space-y-2 border border-[#E2E8F0] dark:border-gray-600 text-[11px]">
                <div>
                  <p className="text-[#334155] dark:text-gray-200 font-bold mb-1">Monto en {metodosPago.find(m => m.metodoPagoId === metodoPagoId)?.nombre ?? 'Efectivo'}</p>
                  <div className="flex items-center gap-1">
                    <span className="text-slate-400 dark:text-gray-500 font-bold">S/</span>
                    <input
                      type="number"
                      min="0.10"
                      step="0.01"
                      value={montoMetodo1}
                      onChange={e => setMontoMetodo1(e.target.value)}
                      placeholder="0.00"
                      className="w-full bg-white dark:bg-gray-700 border border-[#E2E8F0] dark:border-gray-600 text-[#1E293B] dark:text-gray-100 rounded px-2 py-1 text-xs focus:outline-none focus:border-[#7C2D12] dark:focus:border-orange-500 font-semibold"
                    />
                  </div>
                </div>
                <div>
                  <p className="text-[#334155] dark:text-gray-200 font-bold mb-1">Elegir segundo método</p>
                  <div className="flex gap-1.5 flex-wrap">
                    {metodosPago
                      .filter(m => m.metodoPagoId !== metodoPagoId)
                      .map(m => (
                        <button
                          key={m.metodoPagoId}
                          onClick={() => setMetodoPago2Id(m.metodoPagoId)}
                          className={`px-2 py-1 rounded font-bold border transition text-xs ${
                            metodoPago2Id === m.metodoPagoId
                              ? getPaymentColorClass(m.nombre)
                              : 'bg-white dark:bg-gray-700 text-[#334155]/70 dark:text-gray-300 border-[#E2E8F0] dark:border-gray-600 hover:border-[#7C2D12]/40 dark:hover:border-orange-500 hover:text-[#1E293B] dark:hover:text-white'
                          }`}
                        >
                          {m.nombre}
                        </button>
                      ))}
                  </div>
                  {montoMetodo1 && parseFloat(montoMetodo1) > 0 && parseFloat(montoMetodo1) < total && (
                    <p className="text-[#7C2D12] dark:text-orange-400 font-extrabold text-[10px] sm:text-right mt-1.5">
                      Resta para método 2: {formatSoles(total - parseFloat(montoMetodo1))}
                    </p>
                  )}
                </div>
              </div>
            )}
          </div>

          {/* Vuelto Calculation block (only for single Cash payments) */}
          {!pagoDoble && isEfectivo && (
            <div className="bg-[#F8FAFC] dark:bg-gray-700/50 rounded-xl p-2.5 space-y-1.5 border border-[#E2E8F0] dark:border-gray-600 transition-colors">
              <label htmlFor="monto-recibido" className="text-[10px] text-[#1E293B] dark:text-gray-300 uppercase tracking-wider font-extrabold block">
                Efectivo Recibido
              </label>
              <div className="flex items-center gap-1.5">
                <span className="text-[#7C2D12] dark:text-orange-400 font-extrabold text-sm">S/</span>
                <input
                  id="monto-recibido"
                  type="number"
                  min="0"
                  step="0.10"
                  value={montoEfectivo}
                  onChange={e => setMontoEfectivo(e.target.value)}
                  placeholder="Monto entregado"
                  className="flex-1 bg-white dark:bg-gray-700 border border-[#E2E8F0] dark:border-gray-600 rounded-lg px-2.5 py-1 text-xs text-[#1E293B] dark:text-gray-100 font-semibold placeholder-[#334155]/40 dark:placeholder-gray-500 focus:outline-none focus:border-[#7C2D12] dark:focus:border-orange-500"
                />
              </div>
              {valMontoEfectivo > 0 && (
                <div className={`flex justify-between items-center text-xs font-bold pt-1 ${vuelto >= 0 ? 'text-[#10B981]' : 'text-rose-500'}`}>
                  <span>{vuelto >= 0 ? 'Vuelto Cajero:' : 'Falta cobrar:'}</span>
                  <span>{formatSoles(Math.abs(vuelto))}</span>
                </div>
              )}
            </div>
          )}

          {/* Totales Block matching Clean Minimalism */}
          <div className="space-y-1 text-[#334155]/80 dark:text-gray-400 text-[11px] pt-1.5 border-t border-[#E2E8F0] dark:border-gray-700">
            <div className="flex justify-between">
              <span>Subtotal Gravado (S/.):</span>
              <span className="font-extrabold text-[#1E293B] dark:text-gray-100">{formatSoles(subtotal)}</span>
            </div>
            <div className="flex justify-between">
              <span>IGV (18% S/.):</span>
              <span className="font-extrabold text-[#1E293B] dark:text-gray-100">{formatSoles(igv)}</span>
            </div>
            <div className="flex justify-between font-extrabold text-[#1E293B] dark:text-gray-100 border-t border-[#E2E8F0] dark:border-gray-700 pt-1.5 text-xs tracking-wide">
              <span>TOTAL A PAGAR:</span>
              <span className="text-base text-[#7C2D12] dark:text-orange-500 font-black">{formatSoles(total)}</span>
            </div>
          </div>

          {/* Action Call to action payment trigger */}
          <div className="pt-2">
            {errorMsg && (
              <p className="text-rose-500 text-xs text-center font-bold mb-2 animate-headShake">
                {errorMsg}
              </p>
            )}
            <button
              onClick={handleCobrar}
              disabled={cobrarDisabled}
              className="w-full py-3.5 rounded-xl bg-[#10B981] hover:bg-[#059669] text-white font-extrabold text-sm shadow-xs active:scale-95 disabled:opacity-35 disabled:scale-100 disabled:shadow-none transition flex items-center justify-center gap-2 cursor-pointer uppercase tracking-wider"
            >
              {processing ? (
                <>
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  <span>Registrando...</span>
                </>
              ) : (
                <>
                  <ShoppingCart size={16} />
                  <span>Registrar Venta</span>
                </>
              )}
            </button>
          </div>

        </div>

      </div>
    </div>
  )
}
