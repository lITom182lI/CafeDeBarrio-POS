import { getDb } from './db'
import type { PendingTransaction } from '../types'

export async function savePending(tx: PendingTransaction): Promise<void> {
  const db = await getDb()
  await db.put('pendingTransactions', tx)
}

export async function getPending(): Promise<PendingTransaction[]> {
  const db = await getDb()
  const all = await db.getAllFromIndex('pendingTransactions', 'by-sincronizada', 0)
  return all
}

export async function markSynced(
  localId: string,
  transaccionIdServidor: number
): Promise<void> {
  const db = await getDb()
  const tx = await db.get('pendingTransactions', localId)
  if (!tx) return
  await db.put('pendingTransactions', {
    ...tx,
    sincronizada: 1,
    transaccionIdServidor,
    error: null,
  })
}

export async function markError(localId: string, error: string): Promise<void> {
  const db = await getDb()
  const tx = await db.get('pendingTransactions', localId)
  if (!tx) return
  await db.put('pendingTransactions', { ...tx, error })
}

export async function getPendingCount(): Promise<number> {
  const pending = await getPending()
  return pending.length
}
