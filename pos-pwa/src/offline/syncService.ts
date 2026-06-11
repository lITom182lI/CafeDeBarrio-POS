import { getPending, markSynced, markError, getPendingCount } from './pendingStore'
import { crearTransaccion, OfflineError } from '../api'
import type { CreateTransaccionRequest } from '../types'

type SyncCallback = (pendingCount: number) => void

let _timer: ReturnType<typeof setInterval> | null = null
let _onSync: SyncCallback | null = null

export function startSync(onSync: SyncCallback, intervalMs = 30_000): void {
  _onSync = onSync
  if (_timer) return
  // Primera ejecución a los 10s, luego cada 30s
  setTimeout(() => syncNow(), 10_000)
  _timer = setInterval(() => syncNow(), intervalMs)
}

export function stopSync(): void {
  if (_timer) { clearInterval(_timer); _timer = null }
}

export async function syncNow(): Promise<void> {
  const pending = await getPending()
  if (pending.length === 0) return

  let algoCambio = false

  for (const tx of pending) {
    try {
      const request: CreateTransaccionRequest = {
        sedeId:          tx.sedeId,
        clienteId:       null,
        metodoPagoId:    tx.metodoPagoId,
        items:           tx.items,
        operadorId:      tx.operadorId,
        tipoDocumento:   tx.tipoDocumento,
        numeroDocumento: tx.numeroDocumento,
        razonSocial:     tx.razonSocial,
        idempotencyKey:  tx.idempotencyKey,
      }
      const id = await crearTransaccion(request)
      await markSynced(tx.localId, id)
      algoCambio = true
    } catch (err) {
      if (err instanceof OfflineError) break  // sin internet, parar
      await markError(tx.localId, err instanceof Error ? err.message : 'Error')
    }
  }

  if (algoCambio && _onSync) {
    _onSync(await getPendingCount())
  }
}
