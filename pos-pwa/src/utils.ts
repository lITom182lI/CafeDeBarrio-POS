import type { CartItem, Totales } from './types'
import { config } from './config'

export function calcularTotales(cart: CartItem[]): Totales {
  const total = cart.reduce((acc, i) => acc + i.precio * i.cantidad, 0)
  const tasaCalculo = 1 + config.tasaIgv
  const subtotal = Math.round((total / tasaCalculo) * 100) / 100
  const igv = Math.round((total - subtotal) * 100) / 100
  return { subtotal, igv, total }
}

export function formatSoles(n: number): string {
  return `S/ ${n.toFixed(2)}`
}

export function generateLocalId(): string {
  return `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`
}

export function formatHora(iso: string): string {
  return new Date(iso).toLocaleTimeString('es-PE', {
    hour: '2-digit', minute: '2-digit'
  })
}
