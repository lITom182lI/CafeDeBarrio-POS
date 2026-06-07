import { getDb } from './db'
import type { ProductoDto, CategoriaDto, MetodoPagoDto } from '../types'

export async function saveCatalogProductos(items: ProductoDto[]): Promise<void> {
  const db = await getDb()
  const tx = db.transaction('catalogProductos', 'readwrite')
  await tx.store.clear()
  for (const item of items) await tx.store.put(item)
  await tx.done
}

export async function saveCatalogCategorias(items: CategoriaDto[]): Promise<void> {
  const db = await getDb()
  const tx = db.transaction('catalogCategorias', 'readwrite')
  await tx.store.clear()
  for (const item of items) await tx.store.put(item)
  await tx.done
}

export async function saveCatalogMetodosPago(items: MetodoPagoDto[]): Promise<void> {
  const db = await getDb()
  const tx = db.transaction('catalogMetodosPago', 'readwrite')
  await tx.store.clear()
  for (const item of items) await tx.store.put(item)
  await tx.done
}

export async function getCatalogProductos(): Promise<ProductoDto[]> {
  const db = await getDb()
  return db.getAll('catalogProductos')
}

export async function getCatalogCategorias(): Promise<CategoriaDto[]> {
  const db = await getDb()
  return db.getAll('catalogCategorias')
}

export async function getCatalogMetodosPago(): Promise<MetodoPagoDto[]> {
  const db = await getDb()
  return db.getAll('catalogMetodosPago')
}
