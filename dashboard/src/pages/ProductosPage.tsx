import { useState, useEffect, useMemo, useRef } from 'react'
import { api } from '../api/client'
import type { ProductoDto } from '../types'
import { ProductoModal } from '../components/ProductoModal'

function catBadge(nombre?: string) {
  if (!nombre) return 'badge badge-gray'
  const n = nombre.toLowerCase()
  if (n.includes('beb') || n.includes('cafe') || n.includes('café')) return 'badge badge-blue'
  if (n.includes('paste') || n.includes('torta') || n.includes('dulce')) return 'badge badge-pink'
  if (n.includes('comida') || n.includes('sanwich') || n.includes('sand')) return 'badge badge-green'
  return 'badge badge-gray'
}

function ProgressBar({ percentage, isLow }: { percentage: number; isLow: boolean }) {
  const ref = useRef<HTMLDivElement>(null)
  useEffect(() => { if (ref.current) ref.current.style.width = `${percentage}%` }, [percentage])
  return <div ref={ref} className={`bar-fill${isLow ? ' bar-low' : ''}`} />
}

export function ProductosPage() {
  const [productos, setProductos] = useState<ProductoDto[]>([])
  const [busqueda, setBusqueda]   = useState('')
  const [categoria, setCategoria] = useState('Todas')
  const [err, setErr]             = useState('')
  const [modal, setModal]         = useState<{ open: boolean; producto?: ProductoDto | null }>({ open: false })

  const cargar = () => api.productos().then(setProductos).catch(() => setErr('No se pudo cargar el catalogo'))
  useEffect(() => { cargar() }, [])

  const categorias = useMemo(() => {
    const cats = [...new Set(productos.map(p => p.categoriaNombre ?? 'Sin categoria'))]
    return ['Todas', ...cats]
  }, [productos])

  const filtrados = useMemo(() =>
    productos.filter(p => {
      const matchB = p.nombre.toLowerCase().includes(busqueda.toLowerCase())
      const matchC = categoria === 'Todas' || (p.categoriaNombre ?? 'Sin categoria') === categoria
      return matchB && matchC
    }), [productos, busqueda, categoria])

  const barWidth = (p: ProductoDto) => {
    const max = Math.max(p.cantidadDisponible + p.stockMinimo, 1)
    return Math.min((p.cantidadDisponible / max) * 100, 100)
  }
  const isLow = (p: ProductoDto) => p.cantidadDisponible <= p.stockMinimo

  return (
    <div className="page">
      <div className="page-header">
        <h1 className="page-title">Catalogo de Productos</h1>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <div className="page-meta">Total: {filtrados.length} productos</div>
          <button className="btn-primary" onClick={() => setModal({ open: true, producto: null })}>
            + Nuevo Producto
          </button>
        </div>
      </div>

      {err && <div className="error-banner">{err}</div>}

      <div className="search-wrap">
        <span className="search-icon">🔍</span>
        <input placeholder="Buscar producto..." value={busqueda}
          onChange={e => setBusqueda(e.target.value)} />
      </div>

      <div className="filter-row">
        <select aria-label="Filtrar por categoria" className="filter-select"
          value={categoria} onChange={e => setCategoria(e.target.value)}>
          {categorias.map(c => <option key={c}>{c}</option>)}
        </select>
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Producto</th><th>Categoria</th><th>Precio</th><th>Stock</th>
              <th>Estado</th><th></th>
            </tr>
          </thead>
          <tbody>
            {filtrados.length === 0
              ? <tr><td colSpan={6}><div className="empty-state">Sin resultados</div></td></tr>
              : filtrados.map(p => (
                <tr key={p.productoId} className={`data-row${!p.activo ? ' row-inactive' : ''}`}>
                  <td className="fw-medium">{p.nombre}</td>
                  <td><span className={catBadge(p.categoriaNombre)}>{p.categoriaNombre ?? '—'}</span></td>
                  <td>S/ {p.precio.toFixed(2)}</td>
                  <td>
                    <div className="stock-cell">
                      <span className={`stock-num${isLow(p) ? ' dif-neg' : ''}`}>
                        {p.cantidadDisponible.toFixed(0)}
                      </span>
                      <div className="bar-bg">
                        <ProgressBar percentage={barWidth(p)} isLow={isLow(p)} />
                      </div>
                    </div>
                  </td>
                  <td>
                    <span className={`badge ${p.activo ? 'badge-green' : 'badge-gray'}`}>
                      {p.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td>
                    <button className="btn-link"
                      onClick={() => setModal({ open: true, producto: p })}>
                      Editar
                    </button>
                  </td>
                </tr>
              ))
            }
          </tbody>
        </table>
      </div>

      {modal.open && (
        <ProductoModal
          producto={modal.producto}
          onClose={() => setModal({ open: false })}
          onSaved={() => { setModal({ open: false }); cargar() }}
        />
      )}
    </div>
  )
}
