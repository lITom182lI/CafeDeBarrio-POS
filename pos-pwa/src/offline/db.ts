import { openDB, type DBSchema, type IDBPDatabase } from 'idb'
import type { PendingTransaction } from '../types'

interface CafeDB extends DBSchema {
  pendingTransactions: {
    key: string
    value: PendingTransaction
    indexes: { 'by-sincronizada': number }
  }
}

let _db: IDBPDatabase<CafeDB> | null = null

export async function getDb(): Promise<IDBPDatabase<CafeDB>> {
  if (_db) return _db
  _db = await openDB<CafeDB>('cafe-barrio-pos', 1, {
    upgrade(db) {
      const store = db.createObjectStore('pendingTransactions', {
        keyPath: 'localId',
      })
      store.createIndex('by-sincronizada', 'sincronizada')
    },
  })
  return _db
}
