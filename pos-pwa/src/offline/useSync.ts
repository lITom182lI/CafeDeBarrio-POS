import { useEffect, useState, useCallback, startTransition } from 'react'
import { startSync, stopSync, syncNow } from './syncService'
import { getPendingCount } from './pendingStore'

export function useSync() {
  const [pendingCount, setPendingCount] = useState(0)
  const [isOnline, setIsOnline]         = useState(navigator.onLine)

  const refreshCount = useCallback(async () => {
    setPendingCount(await getPendingCount())
  }, [])

  useEffect(() => {
    startTransition(() => { void refreshCount() })

    const onOnline  = () => { setIsOnline(true);  syncNow() }
    const onOffline = () => setIsOnline(false)

    window.addEventListener('online',  onOnline)
    window.addEventListener('offline', onOffline)

    startSync(count => setPendingCount(count))

    return () => {
      window.removeEventListener('online',  onOnline)
      window.removeEventListener('offline', onOffline)
      stopSync()
    }
  }, [refreshCount])

  return { pendingCount, isOnline, refreshCount }
}
