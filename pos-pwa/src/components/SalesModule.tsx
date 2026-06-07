import { useState, useEffect, useMemo, startTransition } from 'react'
import type {
  ProductoDto, CategoriaDto, MetodoPagoDto,
  CartItem, ComprobanteData, CreateTransaccionRequest, OperadorSession,
} from '../types'
import { getProductos, getCategorias, getMetodosPago, crearTransaccion, OfflineError } from '../api'
import { calcularTotales, formatSoles, generateLocalId } from '../utils'
import { config } from '../config'
import { useSync, savePending } from '../offline'
import {
  saveCatalogProductos, saveCatalogCategorias, saveCatalogMetodosPago,
  getCatalogProductos, getCatalogCategorias, getCatalogMetodosPago,
} from '../offline'
import { useInstallPrompt } from '../hooks/useInstallPrompt'
import ComprobanteModal from './ComprobanteModal'
import TicketModal, { type TicketData } from './TicketModal'

interface Props {
  session: OperadorSession | null
  onLogout: () => void
}

export default function SalesModule({ session, onLogout }: Props) {
  // ── Catálogo ──────────────────────────────────────────────────────────────
  const [productos, setProductos] = useState<ProductoDto[]>([])
  const [categorias, setCategorias] = useState<CategoriaDto[]>([])
  const [metodosPago, setMetodosPago] = useState<MetodoPagoDto[]>([])
  const [loadingCatalog, setLoadingCatalog] = useState(true)
  const [catalogOffline, setCatalogOffline] = useState(false)

  // ── Filtros ───────────────────────────────────────────────────────────────
  const [selectedCat, setSelectedCat] = useState<string | null>(null)
  const [search, setSearch] = useState('')

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

  // ── Sync ──────────────────────────────────────────────────────────────────
  const { pendingCount, isOnline, refreshCount } = useSync()
  const { canInstall, install } = useInstallPrompt()

  // ── Carga del catálogo ────────────────────────────────────────────────────
  async function loadCatalog() {
    setLoadingCatalog(true)
    setCatalogOffline(false)
    try {
      const [prods, cats, metodos] = await Promise.all([
        getProductos(),
        getCategorias(),
        getMetodosPago(),
      ])
      setProductos(prods.filter(p => p.activo))
      setCategorias(cats)
      setMetodosPago(metodos)
      if (metodos.length > 0) setMetodoPagoId(metodos[0].metodoPagoId)
      void saveCatalogProductos(prods)
      void saveCatalogCategorias(cats)
      void saveCatalogMetodosPago(metodos)
    } catch (err) {
      if (err instanceof OfflineError) {
        const [prods, cats, metodos] = await Promise.all([
          getCatalogProductos(),
          getCatalogCategorias(),
          getCatalogMetodosPago(),
        ])
        if (prods.length > 0) {
          setProductos(prods.filter(p => p.activo))
          setCategorias(cats)
          setMetodosPago(metodos)
          if (metodos.length > 0) setMetodoPagoId(metodos[0].metodoPagoId)
        } else {
          setCatalogOffline(true)
        }
      }
    } finally {
      setLoadingCatalog(false)
    }
  }

  useEffect(() => { startTransition(() => { void loadCatalog() }) }, [])

  // ── Productos filtrados ───────────────────────────────────────────────────
  const productosFiltrados = useMemo(() =>
    productos
      .filter(p => selectedCat === null || p.categoriaNombre === selectedCat)
      .filter(p => search === '' || p.nombre.toLowerCase().includes(search.toLowerCase())),
    [productos, selectedCat, search]
  )

  // ── Operaciones del carrito ───────────────────────────────────────────────
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
    setCart(prev =>
      prev
        .map(i => i.productoId === productoId ? { ...i, cantidad: i.cantidad + delta } : i)
        .filter(i => i.cantidad > 0)
    )
  }

  function removeFromCart(productoId: number) {
    setCart(prev => prev.filter(i => i.productoId !== productoId))
  }

  // ── Cálculos derivados ────────────────────────────────────────────────────
  const { subtotal, igv, total } = useMemo(() => calcularTotales(cart), [cart])
  const metodoPagoNombre = metodosPago.find(m => m.metodoPagoId === metodoPagoId)?.nombre ?? ''
  const isEfectivo = metodoPagoNombre.toLowerCase().includes('efectivo')
  const montoEfectivoNum = parseFloat(montoEfectivo) || 0
  const vuelto = montoEfectivoNum - total

  const montoM1 = parseFloat(montoMetodo1) || 0

  const cobrarDisabled =
    cart.length === 0 ||
    metodoPagoId === null ||
    (pagoDoble && (metodoPago2Id === null || metodoPago2Id === metodoPagoId || montoM1 <= 0 || montoM1 >= total)) ||
    (!pagoDoble && isEfectivo && montoEfectivoNum < total) ||
    processing

  // ── Toggle boleta nominada ────────────────────────────────────────────────
  function handleComprobanteToggle(checked: boolean) {
    setWantsComprobante(checked)
    if (checked) setShowComprobanteModal(true)
    else setComprobante(null)
  }

  // ── Procesar venta ────────────────────────────────────────────────────────
  async function handleCobrar() {
    if (cobrarDisabled) return
    setProcessing(true)

    const fechaHora = new Date().toISOString()
    const request: CreateTransaccionRequest = {
      sedeId: config.sedeId,
      clienteId: null,
      metodoPagoId: metodoPagoId!,
      items: cart.map(i => ({ productoId: i.productoId, cantidad: i.cantidad })),
      operadorId: session?.operadorId ?? null,
      tipoDocumento: comprobante?.tipoDocumento ?? null,
      numeroDocumento: comprobante?.numeroDocumento ?? null,
      razonSocial: comprobante?.razonSocial ?? null,
      metodoPagoSecundarioId: pagoDoble ? metodoPago2Id : null,
      montoMetodoPrimario: pagoDoble ? montoM1 : null,
    }

    let offline = false
    let transaccionId: number | undefined
    let localId: string | undefined

    const sinToken = !session?.token

    try {
      if (sinToken) throw new OfflineError()
      transaccionId = await crearTransaccion(request)
    } catch (err) {
      if (err instanceof OfflineError || sinToken) {
        offline = true
        localId = generateLocalId()
        await savePending({
          localId,
          sedeId: config.sedeId,
          metodoPagoId: metodoPagoId!,
          metodoPagoNombre,
          operadorId: session?.operadorId ?? null,
          items: request.items,
          cartSnapshot: [...cart],
          subtotal, igv, total,
          fechaLocal: fechaHora,
          tipoDocumento: comprobante?.tipoDocumento ?? null,
          numeroDocumento: comprobante?.numeroDocumento ?? null,
          razonSocial: comprobante?.razonSocial ?? null,
          sincronizada: 0,
          transaccionIdServidor: null,
          error: null,
        })
        await refreshCount()
      } else {
        alert(`Error: ${err instanceof Error ? err.message : 'Error al registrar la venta'}`)
        setProcessing(false)
        return
      }
    }

    setTicket({ 
      transaccionId, localId, items: [...cart], subtotal, igv, total, metodoPagoNombre, comprobante, fechaHora, offline,
      metodoPagoSecundarioNombre: pagoDoble && metodoPago2Id
        ? (metodosPago.find(m => m.metodoPagoId === metodoPago2Id)?.nombre ?? undefined)
        : undefined,
      montoMetodoPrimario: pagoDoble ? montoM1 : undefined
    })

    // Reset para nueva venta
    setCart([])
    setPagoDoble(false)
    setMetodoPago2Id(null)
    setMontoMetodo1('')
    setMontoEfectivo('')
    setWantsComprobante(false)
    setComprobante(null)
    setProcessing(false)
  }

  // ── Render ────────────────────────────────────────────────────────────────
  return (
    <div className="h-screen flex flex-col bg-stone-100 overflow-hidden">
      {/* Header */}
      <header
        className="flex items-center justify-between px-4 py-2 shadow-sm flex-shrink-0 bg-brand"
      >
        <div className="flex items-center gap-2">
          <span className="text-xl">☕</span>
          <span className="font-bold text-white text-lg hidden sm:inline">Café de Barrio</span>
          <span className="font-bold text-white text-lg sm:hidden">CdB</span>
        </div>
        <div className="flex items-center gap-2">
          {pendingCount > 0 && (
            <span className="bg-amber-400 text-amber-900 text-xs font-bold px-2 py-0.5 rounded-full">
              {pendingCount} pendiente{pendingCount > 1 ? 's' : ''}
            </span>
          )}
          <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${
            isOnline ? 'bg-green-500/20 text-green-200' : 'bg-red-500/20 text-red-300'
          }`}>
            {isOnline ? '● Online' : '○ Offline'}
          </span>
          {session && (
            <span className="text-white/80 text-sm hidden md:inline">👤 {session.nombre}</span>
          )}
          {canInstall && (
            <button
              onClick={install}
              className="text-white/70 hover:text-white text-xs px-2 py-1 rounded border border-white/30 hover:border-white/60 transition-colors hidden sm:inline"
            >
              ⬇ Instalar
            </button>
          )}
          <button
            onClick={onLogout}
            className="text-white/70 hover:text-white text-xs px-2 py-1 rounded border border-white/30 hover:border-white/60 transition-colors"
          >
            Salir
          </button>
        </div>
      </header>

      {/* Body */}
      <div className="flex flex-1 overflow-hidden">
        {/* ── PANEL IZQUIERDO: Catálogo ── */}
        <div className="flex-1 flex flex-col overflow-hidden border-r border-stone-200">
          {/* Búsqueda y categorías */}
          <div className="p-3 space-y-2 bg-white border-b border-stone-200 flex-shrink-0">
            <input
              type="search"
              aria-label="Buscar producto"
              placeholder="Buscar producto..."
              value={search}
              onChange={e => setSearch(e.target.value)}
              className="w-full border border-stone-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:border-amber-500"
            />
            <div className="flex gap-2 overflow-x-auto pb-1">
              <button
                onClick={() => setSelectedCat(null)}
                className={`flex-shrink-0 px-3 py-1 rounded-full text-sm font-medium transition-colors ${
                  selectedCat === null ? 'text-white bg-brand' : 'bg-stone-100 text-stone-600 hover:bg-stone-200'
                }`}
              >
                Todos
              </button>
              {categorias.map(cat => (
                <button
                  key={cat.categoriaId}
                  onClick={() => setSelectedCat(cat.nombre)}
                  className={`flex-shrink-0 px-3 py-1 rounded-full text-sm font-medium transition-colors ${
                    selectedCat === cat.nombre ? 'text-white bg-brand' : 'bg-stone-100 text-stone-600 hover:bg-stone-200'
                  }`}
                >
                  {cat.nombre}
                </button>
              ))}
            </div>
          </div>

          {/* Grid de productos */}
          <div className="flex-1 overflow-y-auto p-3">
            {loadingCatalog && (
              <div className="flex items-center justify-center h-32 text-stone-400 text-sm">
                Cargando catálogo...
              </div>
            )}

            {!loadingCatalog && catalogOffline && (
              <div className="flex flex-col items-center justify-center h-32 gap-3">
                <p className="text-stone-500 text-sm">Sin conexión — catálogo no disponible</p>
                <button onClick={loadCatalog} className="text-amber-700 text-sm hover:underline">
                  Reintentar
                </button>
              </div>
            )}

            {!loadingCatalog && !catalogOffline && productosFiltrados.length === 0 && (
              <div className="text-center text-stone-400 text-sm mt-8">Sin resultados</div>
            )}

            {!loadingCatalog && !catalogOffline && productosFiltrados.length > 0 && (
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-2">
                {productosFiltrados.map(p => {
                  const inCart = cart.find(i => i.productoId === p.productoId)
                  const agotado = p.seguimientoInventario && p.cantidadDisponible === 0
                  return (
                    <button
                      key={p.productoId}
                      onClick={() => !agotado && addToCart(p)}
                      disabled={agotado}
                      className={`text-left rounded-xl p-3 shadow-sm border transition-all ${
                        agotado
                          ? 'bg-stone-200 border-stone-300 opacity-50 cursor-not-allowed'
                          : inCart
                          ? 'bg-amber-50 border-amber-400 ring-1 ring-amber-300'
                          : 'bg-white border-stone-200 hover:border-amber-400 hover:shadow-md active:scale-95'
                      }`}
                    >
                      <p className="font-medium text-stone-800 text-sm leading-tight">{p.nombre}</p>
                      <p className="font-bold text-sm mt-1 text-brand">
                        {formatSoles(p.precio)}
                      </p>
                      {p.seguimientoInventario && !agotado && p.cantidadDisponible <= 5 && (
                        <p className="text-orange-500 text-xs mt-1">Quedan {p.cantidadDisponible}</p>
                      )}
                      {agotado && <p className="text-red-500 text-xs mt-1">Agotado</p>}
                      {inCart && (
                        <span className="inline-block bg-amber-500 text-white text-xs font-bold px-1.5 py-0.5 rounded-full mt-1">
                          {inCart.cantidad} en pedido
                        </span>
                      )}
                    </button>
                  )
                })}
              </div>
            )}
          </div>
        </div>

        {/* ── PANEL DERECHO: Orden y pago ── */}
        <div className="w-72 xl:w-80 flex flex-col bg-white overflow-hidden flex-shrink-0">
          {/* Carrito */}
          <div className="flex-1 overflow-y-auto p-3">
            <h2 className="font-semibold text-stone-600 text-xs uppercase tracking-wide mb-2">Pedido actual</h2>
            {cart.length === 0 ? (
              <div className="text-center text-stone-400 text-sm py-10">
                Toca un producto para agregar
              </div>
            ) : (
              <div className="space-y-0.5">
                {cart.map(item => (
                  <div key={item.productoId} className="flex items-center gap-1.5 py-2 border-b border-stone-100">
                    <div className="flex-1 min-w-0">
                      <p className="text-stone-800 text-xs font-medium truncate">{item.nombre}</p>
                      <p className="text-stone-400 text-xs">{formatSoles(item.precio)}</p>
                    </div>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => updateQty(item.productoId, -1)}
                        className="w-5 h-5 rounded-full bg-stone-100 hover:bg-stone-200 text-stone-600 text-xs font-bold flex items-center justify-center"
                      >
                        −
                      </button>
                      <span className="w-5 text-center text-xs font-bold text-stone-800">{item.cantidad}</span>
                      <button
                        onClick={() => {
                          const prod = productos.find(p => p.productoId === item.productoId)
                          if (prod) addToCart(prod)
                        }}
                        className="w-5 h-5 rounded-full bg-stone-100 hover:bg-stone-200 text-stone-600 text-xs font-bold flex items-center justify-center"
                      >
                        +
                      </button>
                    </div>
                    <span className="text-stone-700 text-xs font-semibold w-14 text-right">
                      {formatSoles(item.precio * item.cantidad)}
                    </span>
                    <button
                      onClick={() => removeFromCart(item.productoId)}
                      className="text-stone-300 hover:text-red-500 text-base leading-none"
                    >
                      ×
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Totales + pago */}
          <div className="border-t border-stone-200 p-3 space-y-3 flex-shrink-0">
            {/* Totales */}
            <div className="space-y-1 text-sm">
              <div className="flex justify-between text-stone-500">
                <span>Subtotal</span><span>{formatSoles(subtotal)}</span>
              </div>
              <div className="flex justify-between text-stone-500">
                <span>IGV ({(config.tasaIgv * 100).toFixed(1)}%)</span>
                <span>{formatSoles(igv)}</span>
              </div>
              <div className="flex justify-between font-bold text-stone-900 text-base pt-1 border-t border-stone-200">
                <span>TOTAL</span><span>{formatSoles(total)}</span>
              </div>
            </div>

            {/* Método de pago */}
            <div className="space-y-2">
              <div className="flex gap-2">
                {metodosPago.map(m => (
                  <button
                    key={m.metodoPagoId}
                    onClick={() => { setMetodoPagoId(m.metodoPagoId); setMontoEfectivo('') }}
                    className={`flex-1 py-2 rounded-lg text-xs font-semibold border transition-colors ${
                      metodoPagoId === m.metodoPagoId
                        ? 'text-white border-brand bg-brand'
                        : 'bg-stone-50 text-stone-600 border-stone-200 hover:border-stone-400'
                    }`}
                  >
                    {m.nombre}
                  </button>
                ))}
              </div>

              {/* Pago doble */}
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={pagoDoble}
                  onChange={e => { setPagoDoble(e.target.checked); setMetodoPago2Id(null); setMontoMetodo1('') }}
                  className="w-4 h-4 accent-amber-600"
                />
                <span className="text-stone-600 text-xs">Pago doble (dos métodos)</span>
              </label>

              {pagoDoble && (
                <div className="bg-stone-50 rounded-lg p-2 space-y-2 border border-stone-200">
                  <div>
                    <p className="text-stone-500 text-xs mb-1">Monto con {metodosPago.find(m => m.metodoPagoId === metodoPagoId)?.nombre ?? '—'}</p>
                    <div className="flex items-center gap-1.5">
                      <span className="text-stone-500 text-sm">S/</span>
                      <input
                        type="number"
                        min="0.01"
                        step="0.01"
                        max={total - 0.01}
                        value={montoMetodo1}
                        onChange={e => setMontoMetodo1(e.target.value)}
                        placeholder="0.00"
                        className="flex-1 border border-stone-300 rounded px-2 py-1 text-sm focus:outline-none focus:border-amber-500"
                      />
                    </div>
                  </div>
                  <div>
                    <p className="text-stone-500 text-xs mb-1">Segundo método</p>
                    <div className="flex gap-1 flex-wrap">
                      {metodosPago
                        .filter(m => m.metodoPagoId !== metodoPagoId)
                        .map(m => (
                          <button
                            key={m.metodoPagoId}
                            onClick={() => setMetodoPago2Id(m.metodoPagoId)}
                            className={`px-2 py-1 rounded-lg text-xs font-semibold border transition-colors ${
                              metodoPago2Id === m.metodoPagoId
                                ? 'text-white border-brand bg-brand'
                                : 'bg-white text-stone-600 border-stone-200 hover:border-stone-400'
                            }`}
                          >
                            {m.nombre}
                          </button>
                        ))}
                    </div>
                    {montoMetodo1 && parseFloat(montoMetodo1) > 0 && parseFloat(montoMetodo1) < total && (
                      <p className="text-stone-400 text-xs mt-1">
                        Resta: {formatSoles(total - parseFloat(montoMetodo1))}
                      </p>
                    )}
                  </div>
                </div>
              )}
            </div>

            {/* Vuelto (solo efectivo) */}
            {!pagoDoble && isEfectivo && (
              <div className="bg-stone-50 rounded-lg p-2 space-y-1">
                <label htmlFor="monto-efectivo" className="text-stone-500 text-xs">Monto recibido</label>
                <div className="flex items-center gap-1.5">
                  <span className="text-stone-500 text-sm">S/</span>
                  <input
                    id="monto-efectivo"
                    type="number"
                    min="0"
                    step="0.50"
                    value={montoEfectivo}
                    onChange={e => setMontoEfectivo(e.target.value)}
                    placeholder="0.00"
                    className="flex-1 border border-stone-300 rounded px-2 py-1 text-sm focus:outline-none focus:border-amber-500"
                  />
                </div>
                {montoEfectivoNum > 0 && (
                  <div className={`flex justify-between text-sm font-semibold ${vuelto >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                    <span>Vuelto</span>
                    <span>{formatSoles(Math.max(0, vuelto))}</span>
                  </div>
                )}
              </div>
            )}

            {/* Boleta nominada */}
            <label htmlFor="check-boleta" className="flex items-start gap-2 cursor-pointer">
              <input
                id="check-boleta"
                type="checkbox"
                checked={wantsComprobante}
                onChange={e => handleComprobanteToggle(e.target.checked)}
                className="mt-0.5 w-4 h-4 accent-amber-600"
              />
              <span className="text-stone-600 text-xs leading-tight">
                Emitir boleta nominada
                {comprobante && (
                  <button
                    onClick={e => { e.preventDefault(); setShowComprobanteModal(true) }}
                    className="block text-amber-700 hover:underline mt-0.5"
                  >
                    {comprobante.tipoDocumento}: {comprobante.numeroDocumento}
                  </button>
                )}
              </span>
            </label>

            {/* Botón COBRAR */}
            <button
              onClick={handleCobrar}
              disabled={cobrarDisabled}
              className="w-full py-3.5 rounded-xl font-bold text-white text-base transition-all disabled:opacity-40 disabled:cursor-not-allowed bg-brand"
            >
              {processing ? 'Procesando...' : `COBRAR ${formatSoles(total)}`}
            </button>
          </div>
        </div>
      </div>

      {/* Modals */}
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
    </div>
  )
}
