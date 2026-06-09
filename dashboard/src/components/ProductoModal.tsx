import { useState, useEffect, useRef } from 'react'
import { api } from '../api/client'
import type { ProductoDto, CategoriaCafeDto, ProductoFormData } from '../types'

const UNIDADES = ['unidad', 'kg', 'litro', 'porcion', 'caja']

const EMPTY: ProductoFormData = {
  nombre: '', descripcion: '', costo: 0, precio: 0,
  cantidadDisponible: 0, stockMinimo: 0, unidadMedida: 'unidad',
  seguimientoInventario: true, esMayorista: false, categoriaId: 0, activo: true
}

interface Props {
  producto?: ProductoDto | null
  onClose: () => void
  onSaved: () => void
}

export function ProductoModal({ producto, onClose, onSaved }: Props) {
  const [form, setForm] = useState<ProductoFormData>(
    producto
      ? {
          nombre: producto.nombre, descripcion: producto.descripcion ?? '',
          costo: 0, precio: producto.precio,
          cantidadDisponible: producto.cantidadDisponible,
          stockMinimo: producto.stockMinimo, unidadMedida: producto.unidadMedida,
          seguimientoInventario: true, esMayorista: false,
          categoriaId: producto.categoriaId ?? 0, activo: producto.activo
        }
      : EMPTY
  )
  const [categorias, setCategorias] = useState<CategoriaCafeDto[]>([])
  const [saving, setSaving] = useState(false)
  const [err, setErr] = useState('')
  const dialogRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    dialogRef.current?.focus()
  }, [])

  useEffect(() => {
    api.categorias().then(setCategorias).catch(() => {})
  }, [])

  const set = (k: keyof ProductoFormData, v: unknown) =>
    setForm(f => ({ ...f, [k]: v }))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!form.nombre.trim()) { setErr('El nombre es obligatorio'); return }
    if (form.precio <= 0)    { setErr('El precio debe ser mayor a 0'); return }
    if (form.categoriaId === 0) { setErr('Selecciona una categoria'); return }
    setSaving(true); setErr('')
    try {
      if (producto) {
        await api.actualizarProducto(producto.productoId, form)
      } else {
        await api.crearProducto(form)
      }
      onSaved()
    } catch (e: unknown) {
      setErr(e instanceof Error ? e.message : 'Error al guardar')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="modal-overlay" role="presentation" onClick={onClose}>
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="producto-modal-title"
        ref={dialogRef}
        tabIndex={-1}
        className="modal-box"
        onClick={e => e.stopPropagation()}
      >
        <div className="modal-header">
          <h2 id="producto-modal-title">{producto ? 'Editar Producto' : 'Nuevo Producto'}</h2>
          <button className="modal-close" aria-label="Cerrar modal" onClick={onClose}>✕</button>
        </div>
        <form onSubmit={handleSubmit} className="modal-form">
          {err && <div className="error-banner" role="alert">{err}</div>}

          <label htmlFor="producto-nombre">Nombre *</label>
          <input id="producto-nombre" value={form.nombre} onChange={e => set('nombre', e.target.value)} required />

          <label htmlFor="producto-descripcion">Descripcion</label>
          <input id="producto-descripcion" value={form.descripcion} onChange={e => set('descripcion', e.target.value)} />

          <div className="form-row">
            <div>
              <label htmlFor="producto-precio">Precio (S/) *</label>
              <input id="producto-precio" type="number" min="0" step="0.01" value={form.precio}
                onChange={e => set('precio', parseFloat(e.target.value) || 0)} required />
            </div>
            <div>
              <label htmlFor="producto-costo">Costo (S/)</label>
              <input id="producto-costo" type="number" min="0" step="0.01" value={form.costo}
                onChange={e => set('costo', parseFloat(e.target.value) || 0)} />
            </div>
          </div>

          <div className="form-row">
            <div>
              <label htmlFor="producto-stock">Stock disponible</label>
              <input id="producto-stock" type="number" min="0" step="0.001" value={form.cantidadDisponible}
                onChange={e => set('cantidadDisponible', parseFloat(e.target.value) || 0)} />
            </div>
            <div>
              <label htmlFor="producto-stock-minimo">Stock minimo</label>
              <input id="producto-stock-minimo" type="number" min="0" step="0.001" value={form.stockMinimo}
                onChange={e => set('stockMinimo', parseFloat(e.target.value) || 0)} />
            </div>
          </div>

          <div className="form-row">
            <div>
              <label htmlFor="producto-unidad">Unidad</label>
              <select id="producto-unidad" value={form.unidadMedida} onChange={e => set('unidadMedida', e.target.value)}>
                {UNIDADES.map(u => <option key={u}>{u}</option>)}
              </select>
            </div>
            <div>
              <label htmlFor="producto-categoria">Categoria *</label>
              <select id="producto-categoria" value={form.categoriaId}
                onChange={e => set('categoriaId', parseInt(e.target.value))}>
                <option value={0}>-- Seleccionar --</option>
                {categorias.map(c =>
                  <option key={c.categoriaId} value={c.categoriaId}>{c.nombre}</option>)}
              </select>
            </div>
          </div>

          <div className="form-row">
            <label htmlFor="producto-seguimiento" className="checkbox-label">
              <input id="producto-seguimiento" type="checkbox" checked={form.seguimientoInventario}
                onChange={e => set('seguimientoInventario', e.target.checked)} />
              Seguimiento de inventario
            </label>
            {producto && (
              <label htmlFor="producto-activo" className="checkbox-label">
                <input id="producto-activo" type="checkbox" checked={form.activo}
                  onChange={e => set('activo', e.target.checked)} />
                Activo
              </label>
            )}
          </div>

          <div className="modal-actions">
            <button type="button" className="btn-secondary" onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" className="btn-primary" disabled={saving}>
              {saving ? 'Guardando...' : (producto ? 'Guardar cambios' : 'Crear producto')}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
