import { openDB, type DBSchema, type IDBPDatabase } from 'idb'
import type { PendingTransaction, ProductoDto, CategoriaDto, MetodoPagoDto } from '../types'

interface CafeDB extends DBSchema {
  pendingTransactions: {
    key: string
    value: PendingTransaction
    indexes: { 'by-sincronizada': number }
  }
  catalogProductos: {
    key: number
    value: ProductoDto
  }
  catalogCategorias: {
    key: number
    value: CategoriaDto
  }
  catalogMetodosPago: {
    key: number
    value: MetodoPagoDto
  }
}

let _db: IDBPDatabase<CafeDB> | null = null

export async function getDb(): Promise<IDBPDatabase<CafeDB>> {
  if (_db) return _db
  _db = await openDB<CafeDB>('cafe-barrio-pos', 2, {
    upgrade(db, oldVersion) {
      if (oldVersion < 1) {
        const store = db.createObjectStore('pendingTransactions', {
          keyPath: 'localId',
        })
        store.createIndex('by-sincronizada', 'sincronizada')
      }
      if (oldVersion < 2) {
        db.createObjectStore('catalogProductos',   { keyPath: 'productoId' })
        db.createObjectStore('catalogCategorias',  { keyPath: 'categoriaId' })
        db.createObjectStore('catalogMetodosPago', { keyPath: 'metodoPagoId' })
      }
    },
  })
  return _db
}
