import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { PendingTransaction } from '../types'

vi.mock('../offline/pendingStore', () => ({
  getPending:      vi.fn(),
  markSynced:      vi.fn(),
  markError:       vi.fn(),
  getPendingCount: vi.fn().mockResolvedValue(0),
  savePending:     vi.fn(),
}))

vi.mock('../api', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api')>()
  return { ...actual, crearTransaccion: vi.fn() }
})

import { syncNow } from '../offline/syncService'
import { getPending, markSynced, markError } from '../offline/pendingStore'
import { crearTransaccion } from '../api'
import { OfflineError } from '../adapters/CafeBarrioPosAdapter'

const mockGetPending       = vi.mocked(getPending)
const mockMarkSynced       = vi.mocked(markSynced)
const mockMarkError        = vi.mocked(markError)
const mockCrearTransaccion = vi.mocked(crearTransaccion)

function buildTx(localId = 'tx-1'): PendingTransaction {
  return {
    localId,
    sedeId: 1,
    metodoPagoId: 1,
    metodoPagoNombre: 'Efectivo',
    operadorId: null,
    items: [{ productoId: 1, cantidad: 1 }],
    cartSnapshot: [],
    subtotal: 8.50,
    igv: 0.89,
    total: 9.39,
    fechaLocal: '2026-06-07T12:00:00.000Z',
    tipoDocumento: null,
    numeroDocumento: null,
    razonSocial: null,
    sincronizada: 0,
    transaccionIdServidor: null,
    error: null,
  }
}

beforeEach(() => {
  vi.clearAllMocks()
  mockMarkSynced.mockResolvedValue(undefined)
  mockMarkError.mockResolvedValue(undefined)
})

describe('syncNow', () => {
  it('no llama a la API cuando no hay transacciones pendientes', async () => {
    mockGetPending.mockResolvedValue([])
    await syncNow()
    expect(mockCrearTransaccion).not.toHaveBeenCalled()
  })

  it('llama a markSynced con el ID del servidor al sincronizar exitosamente', async () => {
    mockGetPending.mockResolvedValue([buildTx('local-1')])
    mockCrearTransaccion.mockResolvedValue(42)
    await syncNow()
    expect(mockMarkSynced).toHaveBeenCalledWith('local-1', 42)
    expect(mockMarkError).not.toHaveBeenCalled()
  })

  it('detiene el loop ante OfflineError sin llamar a markError', async () => {
    mockGetPending.mockResolvedValue([buildTx('tx-a'), buildTx('tx-b')])
    mockCrearTransaccion.mockRejectedValue(new OfflineError())
    await syncNow()
    expect(mockCrearTransaccion).toHaveBeenCalledTimes(1)
    expect(mockMarkError).not.toHaveBeenCalled()
  })

  it('llama a markError y continúa con la siguiente transacción ante error no-offline', async () => {
    mockGetPending.mockResolvedValue([buildTx('tx-1'), buildTx('tx-2')])
    mockCrearTransaccion
      .mockRejectedValueOnce(new Error('Validation error'))
      .mockResolvedValueOnce(99)
    await syncNow()
    expect(mockMarkError).toHaveBeenCalledWith('tx-1', 'Validation error')
    expect(mockMarkSynced).toHaveBeenCalledWith('tx-2', 99)
  })
})
