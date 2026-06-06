import { useState, useEffect } from 'react'
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
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-box" onClick={e => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{producto ? 'Editar Producto' : 'Nuevo Producto'}</h2>
          <button className="modal-close" onClick={onClose}>✕</button>
        </div>
        <form onSubmit={handleSubmit} className="modal-form">
          {err && <div className="error-banner">{err}</div>}

          <label>Nombre *
            <input value={form.nombre} onChange={e => set('nombre', e.target.value)} required />
          </label>

          <label>Descripcion
            <input value={form.descripcion} onChange={e => set('descripcion', e.target.value)} />
          </label>

          <div className="form-row">
            <label>Precio (S/) *
              <input type="number" min="0" step="0.01" value={form.precio}
                onChange={e => set('precio', parseFloat(e.target.value) || 0)} required />
            </label>
            <label>Costo (S/)
              <input type="number" min="0" step="0.01" value={form.costo}
                onChange={e => set('costo', parseFloat(e.target.value) || 0)} />
            </label>
          </div>

          <div className="form-row">
            <label>Stock disponible
              <input type="number" min="0" step="0.001" value={form.cantidadDisponible}
                onChange={e => set('cantidadDisponible', parseFloat(e.target.value) || 0)} />
            </label>
            <label>Stock minimo
              <input type="number" min="0" step="0.001" value={form.stockMinimo}
                onChange={e => set('stockMinimo', parseFloat(e.target.value) || 0)} />
            </label>
          </div>

          <div className="form-row">
            <label>Unidad
              <select value={form.unidadMedida} onChange={e => set('unidadMedida', e.target.value)}>
                {UNIDADES.map(u => <option key={u}>{u}</option>)}
              </select>
            </label>
            <label>Categoria *
              <select value={form.categoriaId}
                onChange={e => set('categoriaId', parseInt(e.target.value))}>
                <option value={0}>-- Seleccionar --</option>
                {categorias.map(c =>
                  <option key={c.categoriaId} value={c.categoriaId}>{c.nombre}</option>)}
              </select>
            </label>
          </div>

          <div className="form-row">
            <label className="checkbox-label">
              <input type="checkbox" checked={form.seguimientoInventario}
                onChange={e => set('seguimientoInventario', e.target.checked)} />
              Seguimiento de inventario
            </label>
            {producto && (
              <label className="checkbox-label">
                <input type="checkbox" checked={form.activo}
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
