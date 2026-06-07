import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { TransaccionRow } from '../components/TransaccionRow'
import type { TransaccionListItemDto } from '../types'

vi.mock('../api/client', () => ({
  api: { transaccionDetalle: vi.fn() },
}))

const txBase: TransaccionListItemDto = {
  transaccionId: 1,
  clienteNombre: 'Sin cliente',
  total: 11.05,
  fecha: new Date().toISOString(),
  metodoPago: 'Efectivo',
  anulada: false,
  operadorNombre: 'Pablo',
}

describe('TransaccionRow', () => {
  it('renderiza sin explotar', () => {
    render(<table><tbody><TransaccionRow tx={txBase} /></tbody></table>)
    expect(screen.getByText('TXN001')).toBeInTheDocument()
  })

  it('muestra badge Completada para no anulada', () => {
    render(<table><tbody><TransaccionRow tx={txBase} /></tbody></table>)
    expect(screen.getByText('Completada')).toBeInTheDocument()
  })

  it('muestra badge Anulada', () => {
    render(<table><tbody><TransaccionRow tx={{ ...txBase, anulada: true }} /></tbody></table>)
    expect(screen.getByText('Anulada')).toBeInTheDocument()
  })

  it('muestra dos badges de método cuando hay pago secundario', () => {
    render(
      <table><tbody>
        <TransaccionRow tx={{ ...txBase, metodoPago: 'Efectivo', metodoPagoSecundario: 'Yape' }} />
      </tbody></table>
    )
    expect(screen.getByText('Efectivo')).toBeInTheDocument()
    expect(screen.getByText('Yape')).toBeInTheDocument()
  })
})
