import { describe, it, expect } from 'vitest'
import { calcularTotales, formatSoles, generateLocalId, formatHora } from '../utils'

describe('calcularTotales', () => {
  it('retorna ceros con carrito vacío', () => {
    const result = calcularTotales([])
    expect(result.subtotal).toBe(0)
    expect(result.igv).toBe(0)
    expect(result.total).toBe(0)
  })

  it('calcula correctamente un ítem', () => {
    const cart = [{ productoId: 1, nombre: 'Café', precio: 5, cantidad: 2 }]
    const result = calcularTotales(cart)
    expect(result.subtotal).toBe(10)
    expect(result.igv).toBe(Math.round(10 * 0.105 * 100) / 100)
    expect(result.total).toBe(result.subtotal + result.igv)
  })

  it('suma varios ítems', () => {
    const cart = [
      { productoId: 1, nombre: 'Café', precio: 5, cantidad: 1 },
      { productoId: 2, nombre: 'Torta', precio: 8, cantidad: 3 },
    ]
    const result = calcularTotales(cart)
    expect(result.subtotal).toBe(29)
  })
})

describe('formatSoles', () => {
  it('formatea con prefijo S/', () => {
    expect(formatSoles(10)).toBe('S/ 10.00')
    expect(formatSoles(1.5)).toBe('S/ 1.50')
  })
})

describe('generateLocalId', () => {
  it('genera IDs únicos', () => {
    const id1 = generateLocalId()
    const id2 = generateLocalId()
    expect(id1).not.toBe(id2)
  })
})

describe('formatHora', () => {
  it('devuelve string en formato HH:MM', () => {
    const result = formatHora('2026-06-07T14:30:00.000Z')
    expect(result).toMatch(/^\d{1,2}:\d{2}(?:\s*(?:a\.?\s*m\.?|p\.?\s*m\.?|AM|PM|am|pm))?$/)
  })
})
