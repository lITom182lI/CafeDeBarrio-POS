import { useState, useEffect, startTransition, type FormEvent, useRef } from "react";
import { Plus, Check, AlertTriangle, Info } from "lucide-react";
import { api } from "../api/client";
import type { ProductoDto, CategoriaCafeDto, ProductoFormData } from "../types";

export function MenuEInventario() {
  const [productos, setProductos] = useState<ProductoDto[]>([]);
  const [categorias, setCategorias] = useState<CategoriaCafeDto[]>([]);
  const [selectedCategoria, setSelectedCategoria] = useState<number>(0); // 0 means All
  const [onlyStockBajo, setOnlyStockBajo] = useState<boolean>(false);
  
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>("");
  const [successMsg, setSuccessMsg] = useState<string>("");

  // Modal active states
  const [modalOpen, setModalOpen] = useState<boolean>(false);
  const [editingProducto, setEditingProducto] = useState<ProductoDto | null>(null);

  // Description modal states
  const [descModalOpen, setDescModalOpen] = useState<boolean>(false);
  const [selectedDescProducto, setSelectedDescProducto] = useState<ProductoDto | null>(null);

  // Form states
  const [formNombre, setFormNombre] = useState<string>("");
  const [formDescripcion, setFormDescripcion] = useState<string>("");
  const [formCosto, setFormCosto] = useState<number>(0);
  const [formPrecio, setFormPrecio] = useState<number>(0);
  const [formCantidadDisp, setFormCantidadDisp] = useState<number>(0);
  const [formStockMinimo, setFormStockMinimo] = useState<number>(0);
  const [formUnidadMedida, setFormUnidadMedida] = useState<string>("Unidades");
  const [formCategoriaId, setFormCategoriaId] = useState<number>(1);
  const [formActivo, setFormActivo] = useState<boolean>(true);
  const [formSeguimiento, setFormSeguimiento] = useState<boolean>(true);
  const [formEsMayorista, setFormEsMayorista] = useState<boolean>(false);
  
  const [saving, setSaving] = useState<boolean>(false);
  const [formError, setFormError] = useState<string>("");
  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (modalOpen) dialogRef.current?.focus();
  }, [modalOpen]);

  const loadData = async () => {
    setLoading(true);
    setError("");
    try {
      const [pData, cData] = await Promise.all([
        api.productos(),
        api.categorias()
      ]);
      setProductos(pData);
      setCategorias(cData);
    } catch {
      setError("No se pudo conectar con la API para recopilar el inventario.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    startTransition(() => {
      void loadData();
    });
  }, []);

  const handleOpenNewModal = () => {
    setEditingProducto(null);
    setFormNombre("");
    setFormDescripcion("");
    setFormCosto(0);
    setFormPrecio(0);
    setFormCantidadDisp(0);
    setFormStockMinimo(0);
    setFormUnidadMedida("Unidades");
    setFormCategoriaId(categorias[0]?.categoriaId ?? 1);
    setFormActivo(true);
    setFormSeguimiento(true);
    setFormEsMayorista(false);
    setFormError("");
    setModalOpen(true);
  };

  const handleOpenEditModal = (p: ProductoDto) => {
    setEditingProducto(p);
    setFormNombre(p.nombre);
    setFormDescripcion(p.descripcion ?? "");
    setFormCosto(p.costo ?? 0);
    setFormPrecio(p.precio);
    setFormCantidadDisp(p.cantidadDisponible);
    setFormStockMinimo(p.stockMinimo);
    setFormUnidadMedida(p.unidadMedida || "Unidades");
    setFormCategoriaId(p.categoriaId);
    setFormActivo(p.activo);
    setFormSeguimiento(p.seguimientoInventario ?? true);
    setFormEsMayorista(p.esMayorista ?? false);
    setFormError("");
    setModalOpen(true);
  };

  const handleDeleteProducto = async (p: ProductoDto) => {
    if (window.confirm(`¿Estás seguro de eliminar el producto "${p.nombre}"? Esta acción también lo removerá de la terminal de ventas POS PWA.`)) {
      try {
        await api.eliminarProducto(p.productoId);
        setSuccessMsg("Producto eliminado correctamente");
        setTimeout(() => setSuccessMsg(""), 3000);
        loadData();
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : "Error al eliminar producto");
      }
    }
  };

  const handleSaveProducto = async (e: FormEvent) => {
    e.preventDefault();
    setFormError("");
    
    if (!formNombre.trim()) {
      setFormError("El nombre del producto es obligatorio.");
      return;
    }
    if (formPrecio <= 0) {
      setFormError("El precio de venta debe ser un número positivo.");
      return;
    }
    if (formCosto < 0) {
      setFormError("El costo no puede ser un valor negativo.");
      return;
    }

    setSaving(true);
    const payload: ProductoFormData = {
      nombre: formNombre,
      descripcion: formDescripcion,
      costo: Number(formCosto),
      precio: Number(formPrecio),
      cantidadDisponible: Number(formCantidadDisp),
      stockMinimo: Number(formStockMinimo),
      unidadMedida: formUnidadMedida,
      seguimientoInventario: formSeguimiento,
      esMayorista: formEsMayorista,
      categoriaId: Number(formCategoriaId),
      activo: formActivo
    };

    try {
      if (editingProducto) {
        await api.actualizarProducto(editingProducto.productoId, payload);
        setSuccessMsg(`Producto "${formNombre}" actualizado exitosamente.`);
      } else {
        await api.crearProducto(payload);
        setSuccessMsg(`Producto "${formNombre}" creado con éxito.`);
      }
      setModalOpen(false);
      void loadData();
      
      setTimeout(() => {
        setSuccessMsg("");
      }, 4000);
    } catch {
      setFormError("No se pudo guardar el producto. Inténtalo de nuevo.");
    } finally {
      setSaving(false);
    }
  };

  // Filtration logic
  const filteredProductos = productos.filter((p) => {
    const matchesCategory = selectedCategoria === 0 || p.categoriaId === selectedCategoria;
    const isCritical = p.cantidadDisponible <= p.stockMinimo;
    const matchesStockBajo = !onlyStockBajo || isCritical;
    return matchesCategory && matchesStockBajo;
  });

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Gestión de Inventario y Productos</h1>
        <button className="btn btn-primary-dark" onClick={handleOpenNewModal}>
          <Plus size={16} />
          <span>Nuevo Producto</span>
        </button>
      </div>

      {successMsg && (
        <div className="success-banner">
          <Check size={18} />
          <span>{successMsg}</span>
        </div>
      )}

      {error && (
        <div className="error-banner" role="alert">
          <AlertTriangle size={18} />
          <span>{error}</span>
          <button onClick={() => { void loadData(); }} className="btn-link ml-auto text-danger fw-bold underline">
            Reintentar
          </button>
        </div>
      )}

      {/* Filter box matching Image 2 */}
      <div className="filter-wrap-card">
        <div className="filter-left-col">
          <div className="select-group">
            <span className="select-grouplabel">Categoría de Producto</span>
            <select
              title="Categoría de Producto"
              className="styled-select"
              value={selectedCategoria}
              onChange={(e) => setSelectedCategoria(Number(e.target.value))}
            >
              <option value={0}>Todas las categorías</option>
              {categorias.map((cat) => (
                <option key={cat.categoriaId} value={cat.categoriaId}>
                  {cat.nombre}
                </option>
              ))}
            </select>
          </div>

          <label className="critical-checkbox-wrap" htmlFor="stock-bajo-toggle">
            <input
              id="stock-bajo-toggle"
              type="checkbox"
              checked={onlyStockBajo}
              onChange={(e) => setOnlyStockBajo(e.target.checked)}
            />
            <span className="critical-checkbox-label">
              ⚠️ Mostrar solo stock bajo / crítico
            </span>
          </label>
        </div>

        <div className="total-counter">
          Total filtrados: <span className="total-num">{filteredProductos.length} productos</span>
        </div>
      </div>

      {loading ? (
        <div className="loading-spinner-wrap">
          <div className="spinner"></div>
          <span>Consultando catálogo de productos...</span>
        </div>
      ) : (
        <div className="table-wrap">
          {filteredProductos.length === 0 ? (
            <div className="empty-state">
              No se encontraron productos con los filtros seleccionados.
            </div>
          ) : (
            <table className="pos-table">
              <thead>
                <tr>
                  <th className="w-110">Nombre</th>
                  <th>Categoría</th>
                  <th>Costo</th>
                  <th>Precio Venta</th>
                  <th>Cantidad Disp.</th>
                  <th>Mínimo Seg.</th>
                  <th>Estado</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {filteredProductos.map((p) => {
                  const isCritical = p.cantidadDisponible <= p.stockMinimo;
                  return (
                    <tr key={p.productoId}>
                      <td>
                        <div className="product-name-block">
                          <span className="text-sm font-medium" style={{ textTransform: "uppercase" }}>{p.nombre}</span>
                        </div>
                      </td>
                      <td>
                        <span className="fw-medium">
                          {p.categoriaNombre || "Otros / Bebidas"}
                        </span>
                      </td>
                      <td>
                        <span className="mono-price">
                          S/. {(p.costo ?? 0).toFixed(2)}
                        </span>
                      </td>
                      <td>
                        <span className="mono-price font-bold">
                          S/. {p.precio.toFixed(2)}
                        </span>
                      </td>
                      <td>
                        <span
                          className={`badge-stock ${
                            isCritical ? "badge-stock-critical" : "badge-stock-normal"
                          }`}
                        >
                          {p.cantidadDisponible} {p.unidadMedida}
                        </span>
                      </td>
                      <td>
                        <span className="text-muted fw-semibold">
                          {p.stockMinimo} ({p.unidadMedida})
                        </span>
                      </td>
                      <td>
                        <span
                          className={p.activo ? "badge-habilitado" : "badge-deshabilitado"}
                        >
                          {p.activo ? "Habilitado" : "Inactivo"}
                        </span>
                      </td>
                      <td>
                        <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
                          <button
                            className="btn-action-edit"
                            style={{ padding: "4px" }}
                            onClick={() => {
                              setSelectedDescProducto(p);
                              setDescModalOpen(true);
                            }}
                            title="Ver descripción"
                          >
                            <Info size={18} />
                          </button>
                          <button
                            className="btn-action-edit"
                            onClick={() => handleOpenEditModal(p)}
                          >
                            Editar
                          </button>
                          <button
                            className="btn-action-delete"
                            onClick={() => handleDeleteProducto(p)}
                          >
                            Eliminar
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* Editor Modal Popup */}
      {modalOpen && (
        <div className="modal-overlay" role="presentation" onClick={() => setModalOpen(false)}>
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="product-modal-title"
            ref={dialogRef}
            tabIndex={-1}
            className="modal-box"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h2 id="product-modal-title">
                {editingProducto ? "Editar Producto" : "Nuevo Producto POS"}
              </h2>
              <button
                className="modal-close"
                aria-label="Cerrar modal"
                onClick={() => setModalOpen(false)}
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleSaveProducto} className="modal-form">
              {formError && (
                <div className="error-banner" role="alert">
                  <span>{formError}</span>
                </div>
              )}

              <label htmlFor="prod-nombre">
                Nombre del Producto *
                <input
                  id="prod-nombre"
                  type="text"
                  placeholder="EJ. ESPRESSO DOBLE"
                  value={formNombre}
                  onChange={(e) => setFormNombre(e.target.value.toUpperCase())}
                  required
                />
              </label>

              <label htmlFor="prod-desc">
                Descripción
                <textarea
                  id="prod-desc"
                  rows={2}
                  style={{ resize: "none" }}
                  placeholder="Descripción corta para el punto de venta..."
                  value={formDescripcion}
                  onChange={(e) => setFormDescripcion(e.target.value)}
                />
              </label>

              <div className="form-row">
                <label htmlFor="prod-costo">
                  Costo Unitario (S/.)
                  <input
                    id="prod-costo"
                    type="number"
                    step="0.01"
                    min="0"
                    value={formCosto}
                    onChange={(e) => setFormCosto(Number(e.target.value))}
                  />
                </label>

                <label htmlFor="prod-precio">
                  Precio de Venta (S/.) *
                  <input
                    id="prod-precio"
                    type="number"
                    step="0.01"
                    min="0.01"
                    value={formPrecio}
                    onChange={(e) => setFormPrecio(Number(e.target.value))}
                    required
                  />
                </label>
              </div>

              <div className="form-row">
                <label htmlFor="prod-minimo">
                  Stock de Seguridad Mínimo
                  <input
                    id="prod-minimo"
                    type="number"
                    min="0"
                    value={formStockMinimo}
                    onChange={(e) => setFormStockMinimo(Number(e.target.value))}
                  />
                </label>

                <label htmlFor="prod-unidades">
                  Unidad de Medida
                  <input
                    id="prod-unidades"
                    type="text"
                    value={formUnidadMedida}
                    onChange={(e) => setFormUnidadMedida(e.target.value)}
                  />
                </label>
              </div>

              <div className="form-row">
                <label htmlFor="prod-cant">
                  Cantidad Disponible
                  <input
                    id="prod-cant"
                    type="number"
                    min="0"
                    value={formCantidadDisp}
                    disabled={formSeguimiento === false}
                    onChange={(e) => setFormCantidadDisp(Number(e.target.value))}
                  />
                </label>

                <label htmlFor="prod-cat">
                  Categoría
                  <select
                    id="prod-cat"
                    value={formCategoriaId}
                    onChange={(e) => setFormCategoriaId(Number(e.target.value))}
                  >
                    {categorias.map((c) => (
                      <option key={c.categoriaId} value={c.categoriaId}>
                        {c.nombre}
                      </option>
                    ))}
                  </select>
                </label>
              </div>

              <div className="form-row">
                <label htmlFor="prod-activo" className="checkbox-label">
                  <input
                    id="prod-activo"
                    type="checkbox"
                    checked={formActivo}
                    onChange={(e) => setFormActivo(e.target.checked)}
                  />
                  Producto Habilitado
                </label>

                <label htmlFor="prod-seg" className="checkbox-label">
                  <input
                    id="prod-seg"
                    type="checkbox"
                    checked={formSeguimiento}
                    onChange={(e) => setFormSeguimiento(e.target.checked)}
                  />
                  Controlar Inventario
                </label>
              </div>

              <label htmlFor="prod-mayorista" className="checkbox-label">
                <input
                  id="prod-mayorista"
                  type="checkbox"
                  checked={formEsMayorista}
                  onChange={(e) => setFormEsMayorista(e.target.checked)}
                />
                Es compra mayorista
              </label>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setModalOpen(false)}
                >
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary" disabled={saving}>
                  {saving ? "Guardando..." : "Guardar Producto"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
      {/* Description Modal Popup */}
      {descModalOpen && selectedDescProducto && (
        <div className="modal-overlay" role="presentation" onClick={() => setDescModalOpen(false)}>
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="desc-modal-title"
            className="modal-box"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: "480px" }}
          >
            <div className="modal-header">
              <h2 id="desc-modal-title">Descripción del Producto</h2>
              <button
                className="modal-close"
                aria-label="Cerrar modal"
                onClick={() => setDescModalOpen(false)}
              >
                ✕
              </button>
            </div>
            <div style={{ paddingTop: "8px", color: "var(--color-text)", fontSize: "0.95rem", lineHeight: "1.5" }}>
              <p style={{ marginBottom: "24px" }}>
                <strong style={{ color: "var(--color-text-title)", marginRight: "6px" }}>
                  [{selectedDescProducto.nombre.toUpperCase()}]
                </strong>
                {selectedDescProducto.descripcion || "Sin descripción detallada registrada."}
              </p>
              <button
                type="button"
                className="btn btn-primary"
                onClick={() => setDescModalOpen(false)}
              >
                Aceptar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}