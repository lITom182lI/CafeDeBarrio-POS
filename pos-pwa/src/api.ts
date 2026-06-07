import type { CreateTransaccionRequest } from './types'
export { OfflineError } from './adapters/CafeBarrioPosAdapter'
import { posAdapter } from './adapters/CafeBarrioPosAdapter'

export function setOperadorToken(token: string | null): void {
  posAdapter.setToken(token)
}

export const getProductos     = ()                              => posAdapter.getProductos()
export const getCategorias    = ()                              => posAdapter.getCategorias()
export const getMetodosPago   = ()                              => posAdapter.getMetodosPago()
export const getOperadores    = ()                              => posAdapter.getOperadores()
export const validarPin       = (id: number, pin: string)       => posAdapter.validarPin(id, pin)
export const crearTransaccion = (req: CreateTransaccionRequest) => posAdapter.crearTransaccion(req)
export const checkOnline      = ()                              => posAdapter.checkOnline()
