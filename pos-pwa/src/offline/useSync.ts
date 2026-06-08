import { useEffect, useState, useCallback, startTransition } from 'react'
import { startSync, stopSync, syncNow } from './syncService'
import { getPendingCount } from './pendingStore'

let simulatedOnline = true;
let simulatedOnlineListeners: ((status: boolean) => void)[] = [];

export function getSimulatedOnline() {
  return simulatedOnline;
}

export function setSimulatedOnline(status: boolean) {
  simulatedOnline = status;
  for (const listener of simulatedOnlineListeners) {
    listener(status);
  }
  if (status && navigator.onLine) {
    syncNow();
  }
}

export function useSync() {
  const [pendingCount, setPendingCount] = useState(0)
  const [isOnline, setIsOnline]         = useState(navigator.onLine && simulatedOnline)

  const refreshCount = useCallback(async () => {
    setPendingCount(await getPendingCount())
  }, [])

  useEffect(() => {
    startTransition(() => { void refreshCount() })

    const updateOnlineStatus = () => {
      setIsOnline(navigator.onLine && simulatedOnline);
      if (navigator.onLine && simulatedOnline) {
        syncNow();
      }
    }

    const onOnline  = () => updateOnlineStatus()
    const onOffline = () => updateOnlineStatus()

    window.addEventListener('online',  onOnline)
    window.addEventListener('offline', onOffline)
    
    simulatedOnlineListeners.push(updateOnlineStatus)

    startSync(count => setPendingCount(count))

    return () => {
      window.removeEventListener('online',  onOnline)
      window.removeEventListener('offline', onOffline)
      simulatedOnlineListeners = simulatedOnlineListeners.filter(l => l !== updateOnlineStatus)
      stopSync()
    }
  }, [refreshCount])

  return { pendingCount, isOnline, refreshCount }
}
