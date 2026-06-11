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
      <div className="flex-1 flex flex-col overflow-hidden bg-[#F8FAFC]">
        
        {/* Search header bar - Clean Minimalism style */}
        <div className="pt-4 px-4 pb-2 bg-white border-b border-[#E2E8F0] flex-shrink-0 space-y-2">
          <div className="relative">
            <span className="absolute inset-y-0 left-3 flex items-center text-[#334155]/60">
              <Search size={18} />
            </span>
            <input
              type="text"
              placeholder="Buscar café, té, postres o sandwiches..."
              value={search}
              onChange={e => setSearch(e.target.value)}
              className="w-full bg-[#F8FAFC] border border-[#E2E8F0] rounded-xl pl-10 pr-4 py-3 text-[#1E293B] placeholder-[#334155]/50 font-sans text-sm focus:outline-none focus:border-[#7C2D12] focus:bg-white transition"
            />
          </div>

          {/* Category Quick Chips - Clean Minimalism style */}
          <div className="flex gap-3 overflow-x-auto py-1 scrollbar-none">
            <button
              onClick={() => setSelectedCat(null)}
              className={`flex-shrink-0 px-6 py-3 rounded-xl text-sm tracking-wide font-extrabold transition-all border cursor-pointer uppercase ${
                selectedCat === null
                  ? 'bg-[#7C2D12] text-white border-[#7C2D12] shadow-md'
                  : 'bg-[#F8FAFC] text-[#334155] border-[#E2E8F0] hover:bg-white hover:text-[#1E293B]'
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
                    ? 'bg-[#7C2D12] text-white border-[#7C2D12] shadow-md'
                    : 'bg-[#F8FAFC] text-[#334155] border-[#E2E8F0] hover:bg-white hover:text-[#1E293B]'
                }`}
              >
                {cat.nombre}
              </button>
            ))}
          </div>
        </div>

        {/* Product Catalog Grid - Clean Minimalism style */}
        <div className="flex-1 overflow-y-auto p-4 bg-[#F8FAFC]">
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
                  className={`text-left bg-white rounded-2xl p-4 border transition-all text-[#334155] relative flex flex-col justify-between shadow-xs h-40 group cursor-pointer ${
                    isAgotado
                      ? 'opacity-30 bg-slate-100 border-[#E2E8F0] cursor-not-allowed'
                      : qtyInCart >= p.cantidadDisponible
                      ? 'opacity-80 bg-[#F8FAFC] border-[#E2E8F0] cursor-not-allowed'
                      : qtyInCart > 0
                      ? 'border-[#7C2D12] bg-[#7C2D12]/5 ring-1 ring-[#7C2D12]/20 shadow-xs'
                      : 'border-[#E2E8F0] hover:border-[#7C2D12]/40 hover:shadow-xs hover:transform active:scale-95'
                  }`}
                >
                  {/* Category & Price line */}
                  <div className="flex justify-between items-start w-full">
                    <span className="text-[10px] uppercase tracking-wider text-[#334155]/70 font-semibold bg-[#F1F5F9] px-2 py-0.5 rounded-lg">
                      {p.categoriaNombre}
                    </span>
                    <span className="text-[#7C2D12] font-extrabold text-sm tracking-wide">
                      {formatSoles(p.precio)}
                    </span>
                  </div>

                  {/* Name & Short Description */}
                  <div className="my-2 flex-1 flex flex-col justify-center">
                    <p className="font-extrabold text-[#1E293B] text-sm leading-snug group-hover:text-[#7C2D12] transition">
                      {p.nombre}
                    </p>
                    <p className="text-[11px] text-[#334155]/85 font-sans line-clamp-2 mt-1 leading-relaxed">
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
      <div className="w-[380px] xl:w-[420px] bg-white border-l border-[#E2E8F0] flex flex-col justify-between overflow-hidden shadow-sm flex-shrink-0">
        
        {/* Header order section */}
        <div className="p-4 border-b border-[#E2E8F0] flex items-center justify-between bg-[#F8FAFC] flex-shrink-0">
          <div className="flex items-center gap-1.5 text-[#1E293B] font-extrabold">
            <ShoppingCart size={16} className="text-[#7C2D12]" />
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
              <p className="text-[#1E293B] font-extrabold text-sm">El carrito está vacío.</p>
              <p className="text-[#334155]/60 text-xs max-w-[200px] leading-relaxed">
                Haga clic en los productos para agregarlos.
              </p>
            </div>
          ) : (
            <div className="space-y-1">
              {cart.map(item => {
                const prod = productos.find(p => p.productoId === item.productoId)
                const isMax = prod && item.cantidad >= prod.cantidadDisponible
                return (
                <div key={item.productoId} className="bg-[#F8FAFC] p-2.5 rounded-xl border border-[#E2E8F0] flex items-center justify-between gap-1">
                  <div className="flex-1 min-w-0">
                     <p className="font-extrabold text-[12px] text-[#1E293B] truncate">{item.nombre}</p>
                     <p className="text-[10px] text-[#334155]/60 font-semibold">{formatSoles(item.precio)} c/u</p>
                  </div>
                  <div className="flex items-center gap-1">
                    <button
                      onClick={() => updateQty(item.productoId, -1)}
                      className="w-5 h-5 rounded-lg bg-[#E2E8F0] hover:bg-[#cbd5e0] text-[#334155] text-[10px] font-bold flex items-center justify-center transition active:scale-90 cursor-pointer"
                    >
                      −
                    </button>
                    <span className="w-4 text-center font-extrabold text-xs text-[#1E293B]">{item.cantidad}</span>
                    <button
                      disabled={isMax}
                      onClick={() => updateQty(item.productoId, 1)}
                      className={`w-5 h-5 rounded-lg text-[10px] font-bold flex items-center justify-center transition ${isMax ? 'bg-rose-100 text-rose-300 cursor-not-allowed' : 'bg-[#E2E8F0] hover:bg-[#cbd5e0] text-[#334155] cursor-pointer active:scale-90'}`}
                    >
                      +
                    </button>
                  </div>
                  <div className="w-16 text-right flex-shrink-0 bg">
                    <p className="font-bold text-[#1E293B] text-xs">{formatSoles(item.precio * item.cantidad)}</p>
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
        <div className="p-4 bg-white border-t border-[#E2E8F0] space-y-4 flex-shrink-0">
          
          {/* Metadata: Comprobante Type & Name selectors matching Screenshot 2 */}
          <div className="space-y-2.5">
            <div>
              <label htmlFor="comprobante-select" className="block text-[10px] uppercase tracking-wider text-[#1E293B] font-extrabold mb-1">
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
                className="w-full bg-[#F8FAFC] border border-[#E2E8F0] text-[#1E293B] rounded-lg px-2.5 py-1.5 text-xs font-bold focus:outline-none focus:border-[#7C2D12]"
              >
                <option value="simple">Ticket de Venta Simple</option>
                <option value="boleta">Boleta Nominada o Factura</option>
              </select>
            </div>

            <div>
              <label htmlFor="client-input" className="block text-[10px] uppercase tracking-wider text-[#1E293B] font-extrabold mb-1">
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
                className="w-full bg-[#F8FAFC] border border-[#E2E8F0] text-[#1E293B] rounded-lg px-2.5 py-1.5 text-xs font-bold focus:outline-none focus:border-[#7C2D12] placeholder-slate-400"
              />
              {comprobante && (
                <p className="text-[10px] text-[#7C2D12] font-semibold mt-1">
                  ✅ Listado con {comprobante.tipoDocumento}: {comprobante.numeroDocumento}
                </p>
              )}
            </div>
          </div>

          {/* Payment Methods Selection Stack - Grid 2x2 matched precisely to screenshot 2 */}
          <div className="space-y-1.5">
            <span className="block text-[10px] uppercase tracking-wider text-[#1E293B] font-extrabold mb-1">
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
                        ? 'bg-[#7C2D12] border-[#7C2D12] text-white shadow-2xs'
                        : 'bg-white text-[#334155]/85 border-[#E2E8F0] hover:border-[#7C2D12]/40 hover:text-[#1E293B]'
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
              <span className="text-[#334155]/70 text-[10px] font-bold uppercase tracking-wider">Pago doble (dos formas)</span>
            </label>

            {pagoDoble && (
              <div className="bg-[#F8FAFC] rounded-xl p-2.5 space-y-2 border border-[#E2E8F0] text-[11px]">
                <div>
                  <p className="text-[#334155] font-bold mb-1">Monto en {metodosPago.find(m => m.metodoPagoId === metodoPagoId)?.nombre ?? 'Efectivo'}</p>
                  <div className="flex items-center gap-1">
                    <span className="text-slate-400 font-bold">S/</span>
                    <input
                      type="number"
                      min="0.10"
                      step="0.01"
                      value={montoMetodo1}
                      onChange={e => setMontoMetodo1(e.target.value)}
                      placeholder="0.00"
                      className="w-full bg-white border border-[#E2E8F0] text-[#1E293B] rounded px-2 py-1 text-xs focus:outline-none focus:border-[#7C2D12] font-semibold"
                    />
                  </div>
                </div>
                <div>
                  <p className="text-[#334155] font-bold mb-1">Elegir segundo método</p>
                  <div className="flex gap-1.5 flex-wrap">
                    {metodosPago
                      .filter(m => m.metodoPagoId !== metodoPagoId)
                      .map(m => (
                        <button
                          key={m.metodoPagoId}
                          onClick={() => setMetodoPago2Id(m.metodoPagoId)}
                          className={`px-2 py-1 rounded font-bold border transition text-xs ${
                            metodoPago2Id === m.metodoPagoId
                              ? 'bg-[#7C2D12] text-white border-[#7C2D12]'
                              : 'bg-white text-[#334155] border-[#E2E8F0] hover:border-[#7C2D12]/30'
                          }`}
                        >
                          {m.nombre}
                        </button>
                      ))}
                  </div>
                  {montoMetodo1 && parseFloat(montoMetodo1) > 0 && parseFloat(montoMetodo1) < total && (
                    <p className="text-[#7C2D12] font-extrabold text-[10px] sm:text-right mt-1.5">
                      Resta para método 2: {formatSoles(total - parseFloat(montoMetodo1))}
                    </p>
                  )}
                </div>
              </div>
            )}
          </div>

          {/* Vuelto Calculation block (only for single Cash payments) */}
          {!pagoDoble && isEfectivo && (
            <div className="bg-[#F8FAFC] rounded-xl p-2.5 space-y-1.5 border border-[#E2E8F0]">
              <label htmlFor="monto-recibido" className="text-[10px] text-[#1E293B] uppercase tracking-wider font-extrabold block">
                Efectivo Recibido
              </label>
              <div className="flex items-center gap-1.5">
                <span className="text-[#7C2D12] font-extrabold text-sm">S/</span>
                <input
                  id="monto-recibido"
                  type="number"
                  step="0.10"
                  value={montoEfectivo}
                  onChange={e => setMontoEfectivo(e.target.value)}
                  placeholder="Monto entregado"
                  className="flex-1 bg-white border border-[#E2E8F0] rounded-lg px-2.5 py-1 text-xs text-[#1E293B] font-semibold placeholder-[#334155]/40 focus:outline-none focus:border-[#7C2D12]"
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
          <div className="space-y-1 text-[#334155]/80 text-[11px] pt-1.5 border-t border-[#E2E8F0]">
            <div className="flex justify-between">
              <span>Subtotal Gravado (S/.):</span>
              <span className="font-extrabold text-[#1E293B]">{formatSoles(subtotal)}</span>
            </div>
            <div className="flex justify-between">
              <span>IGV (18% S/.):</span>
              <span className="font-extrabold text-[#1E293B]">{formatSoles(igv)}</span>
            </div>
            <div className="flex justify-between font-extrabold text-[#1E293B] border-t border-[#E2E8F0] pt-1.5 text-xs tracking-wide">
              <span>TOTAL A PAGAR:</span>
              <span className="text-base text-[#7C2D12] font-black">{formatSoles(total)}</span>
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
