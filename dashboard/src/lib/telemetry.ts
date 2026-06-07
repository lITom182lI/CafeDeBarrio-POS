import * as Sentry from '@sentry/react'

const DSN = import.meta.env.VITE_SENTRY_DSN as string | undefined

export function initTelemetry() {
  if (!DSN) return

  Sentry.init({
    dsn: DSN,
    environment: import.meta.env.MODE,
    beforeSend(event) {
      // Masking GDPR — nunca enviar campos sensibles
      if (event.extra) {
        delete event.extra['numeroDocumento']
        delete event.extra['razonSocial']
        delete event.extra['pin']
      }
      return event
    },
  })
}

export function captureError(error: unknown, context?: Record<string, string>) {
  if (DSN) {
    Sentry.captureException(error, { extra: context })
  } else {
    console.error('[Telemetry]', error, context)
  }
}
