import { ErrorBoundary, type FallbackProps } from 'react-error-boundary'
import { captureError } from '../lib/telemetry'
import type { ReactNode } from 'react'

function FallbackUI({ error, resetErrorBoundary }: FallbackProps) {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-stone-100 p-6 text-center">
      <div className="text-4xl mb-4">⚠️</div>
      <h1 className="font-bold text-stone-800 text-lg mb-2">Algo salió mal</h1>
      <p className="text-stone-500 text-sm mb-4 max-w-xs">
        {error instanceof Error ? error.message : String(error)}
      </p>
      <button
        onClick={resetErrorBoundary}
        className="px-4 py-2 rounded-lg bg-brand text-white text-sm font-semibold"
      >
        Reintentar
      </button>
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
