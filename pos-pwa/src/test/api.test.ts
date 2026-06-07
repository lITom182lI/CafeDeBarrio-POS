import { describe, it, expect, vi, beforeEach } from 'vitest'

describe('getProductos — manejo de respuesta paginada vs array', () => {
  const mockFetch = vi.fn()

  beforeEach(() => {
    vi.resetModules()
    vi.stubGlobal('fetch', mockFetch)
  })

  it('extrae items de respuesta paginada { items, totalCount }', async () => {
    const mockItems = [{ productoId: 1, nombre: 'Café', precio: 5, activo: true }]
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ items: mockItems, totalCount: 1 }),
    })
    const { getProductos } = await import('../api')
    const result = await getProductos()
    expect(result).toEqual(mockItems)
  })

  it('maneja respuesta en array directo', async () => {
    const mockItems = [{ productoId: 2, nombre: 'Torta', precio: 8, activo: true }]
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockItems,
    })
    const { getProductos } = await import('../api')
    const result = await getProductos()
    expect(result).toEqual(mockItems)
  })
})
