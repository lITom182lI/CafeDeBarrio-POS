import { ErrorBoundary, type FallbackProps } from 'react-error-boundary'
import { captureError } from '../lib/telemetry'
import type { ReactNode } from 'react'

function FallbackUI({ error, resetErrorBoundary }: FallbackProps) {
  return (
    <div className="fallback-bg">
      <div className="fallback-icon">⚠️</div>
      <h1 className="fallback-title">Algo salió mal</h1>
      <p className="fallback-text">
        {error instanceof Error ? error.message : String(error)}
      </p>
      <button className="btn btn-primary" onClick={resetErrorBoundary}>Reintentar</button>
    </div>
  )
}

export function AppErrorBoundary({ children }: { children: ReactNode }) {
  return (
    <ErrorBoundary
      FallbackComponent={FallbackUI}
      onError={(error, info) => captureError(error, { componentStack: info.componentStack ?? '' })}
    >
      {children}
    </ErrorBoundary>
  )
}
