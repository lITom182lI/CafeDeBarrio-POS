import type { AppConfig } from './types'

export const config: AppConfig = {
  apiUrl:  import.meta.env.VITE_API_URL ?? '',
  sedeId:  Number(import.meta.env.VITE_SEDE_ID ?? '1'),
  tasaIgv: typeof localStorage !== 'undefined'
    ? Number(localStorage.getItem('pos_tasaIgv') ?? '0.18')
    : 0.18,
}

export async function loadRemoteConfig(): Promise<void> {
  try {
    const res = await fetch(
      `${config.apiUrl}/api/configuracion/tasas?sedeId=${config.sedeId}`,
      { signal: AbortSignal.timeout(4000) }
    )
    if (res.ok) {
      const data = await res.json() as { tasaIgv: number }
      config.tasaIgv = data.tasaIgv
      localStorage.setItem('pos_tasaIgv', String(data.tasaIgv))
    }
  } catch {
    // offline — config.tasaIgv ya tiene el valor del cache o el fallback 0.18
  }
}
