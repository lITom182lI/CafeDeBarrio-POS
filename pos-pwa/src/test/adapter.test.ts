import { describe, it, expect, vi, beforeEach } from 'vitest'

describe('CafeBarrioPosAdapter', () => {
  const mockFetch = vi.fn()

  beforeEach(() => {
    vi.resetModules()
    vi.stubGlobal('fetch', mockFetch)
    mockFetch.mockReset()
  })

  describe('OfflineError — detección de desconexión', () => {
    it('lanza OfflineError cuando fetch emite AbortError', async () => {
      const e = new Error('The operation was aborted')
      e.name = 'AbortError'
      mockFetch.mockRejectedValueOnce(e)
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      await expect(new CafeBarrioPosAdapter().getProductos()).rejects.toThrow('Sin conexión con el servidor')
    })

    it('lanza OfflineError cuando fetch emite Failed to fetch', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Failed to fetch'))
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      await expect(new CafeBarrioPosAdapter().getProductos()).rejects.toThrow('Sin conexión con el servidor')
    })

    it('propaga Error de API cuando el servidor responde con error 5xx', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false, status: 500, statusText: 'Internal Server Error',
        text: async () => 'Server error',
      })
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      await expect(new CafeBarrioPosAdapter().getProductos()).rejects.toThrow('API 500')
    })
  })

  describe('getProductos', () => {
    it('extrae items de respuesta paginada { items, totalCount }', async () => {
      const items = [{ productoId: 1, nombre: 'Café', precio: 5, activo: true }]
      mockFetch.mockResolvedValueOnce({ ok: true, status: 200, json: async () => ({ items, totalCount: 1 }) })
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      expect(await new CafeBarrioPosAdapter().getProductos()).toEqual(items)
    })

    it('maneja respuesta array directo', async () => {
      const items = [{ productoId: 2, nombre: 'Torta', precio: 8, activo: true }]
      mockFetch.mockResolvedValueOnce({ ok: true, status: 200, json: async () => items })
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      expect(await new CafeBarrioPosAdapter().getProductos()).toEqual(items)
    })
  })

  describe('validarPin', () => {
    it('retorna null cuando el servidor responde 401', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false, status: 401, statusText: 'Unauthorized',
        text: async () => 'Credenciales incorrectas',
      })
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      expect(await new CafeBarrioPosAdapter().validarPin(1, '1234')).toBeNull()
    })

    it('propaga OfflineError cuando no hay conexión', async () => {
      mockFetch.mockRejectedValueOnce(new Error('NetworkError when attempting to fetch resource.'))
      const { CafeBarrioPosAdapter, OfflineError } = await import('../adapters/CafeBarrioPosAdapter')
      await expect(new CafeBarrioPosAdapter().validarPin(1, '1234')).rejects.toBeInstanceOf(OfflineError)
    })
  })

  describe('setToken', () => {
    it('incluye Authorization header cuando hay token activo', async () => {
      mockFetch.mockResolvedValueOnce({ ok: true, status: 200, json: async () => [] })
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      const adapter = new CafeBarrioPosAdapter()
      adapter.setToken('jwt-test')
      await adapter.getCategorias()
      expect((mockFetch.mock.calls[0][1] as RequestInit).headers as Record<string, string>).toMatchObject({
        Authorization: 'Bearer jwt-test',
      })
    })

    it('omite Authorization header cuando no hay token', async () => {
      mockFetch.mockResolvedValueOnce({ ok: true, status: 200, json: async () => [] })
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      await new CafeBarrioPosAdapter().getCategorias()
      const headers = (mockFetch.mock.calls[0][1] as RequestInit).headers as Record<string, string>
      expect(headers?.Authorization).toBeUndefined()
    })
  })

  describe('checkOnline', () => {
    it('retorna true cuando la API responde correctamente', async () => {
      mockFetch.mockResolvedValueOnce({ ok: true, status: 200, json: async () => [] })
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      expect(await new CafeBarrioPosAdapter().checkOnline()).toBe(true)
    })

    it('retorna false cuando fetch lanza', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Failed to fetch'))
      const { CafeBarrioPosAdapter } = await import('../adapters/CafeBarrioPosAdapter')
      expect(await new CafeBarrioPosAdapter().checkOnline()).toBe(false)
    })
  })
})
