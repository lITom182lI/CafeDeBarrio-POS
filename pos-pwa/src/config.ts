import type { AppConfig } from './types'

export const config: AppConfig = {
  apiUrl:   import.meta.env.VITE_API_URL ?? '',
  sedeId:   Number(import.meta.env.VITE_SEDE_ID ?? '1'),
  tasaIgv:  Number(import.meta.env.VITE_TASA_IGV ?? '0.18'),
}
