import { useState, useEffect, useCallback } from 'react';

interface QueuedRequest {
  id: string;
  method: string;
  url: string;
  data?: unknown;
  headers?: Record<string, string>;
  timestamp: number;
}

const DB_NAME = 'roomsplit-offline';
const STORE_NAME = 'pending-requests';
const DB_VERSION = 1;

function openDB(): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, DB_VERSION);
    request.onupgradeneeded = () => {
      const db = request.result;
      if (!db.objectStoreNames.contains(STORE_NAME)) {
        db.createObjectStore(STORE_NAME, { keyPath: 'id' });
      }
    };
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
}

async function getAllQueued(): Promise<QueuedRequest[]> {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readonly');
    const store = tx.objectStore(STORE_NAME);
    const req = store.getAll();
    req.onsuccess = () => resolve(req.result);
    req.onerror = () => reject(req.error);
  });
}

async function addToQueue(item: QueuedRequest): Promise<void> {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    const store = tx.objectStore(STORE_NAME);
    store.put(item);
    tx.oncomplete = () => resolve();
    tx.onerror = () => reject(tx.error);
  });
}

async function removeFromQueue(id: string): Promise<void> {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    const store = tx.objectStore(STORE_NAME);
    store.delete(id);
    tx.oncomplete = () => resolve();
    tx.onerror = () => reject(tx.error);
  });
}

async function clearQueue(): Promise<void> {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    const store = tx.objectStore(STORE_NAME);
    store.clear();
    tx.oncomplete = () => resolve();
    tx.onerror = () => reject(tx.error);
  });
}

export function useOfflineQueue() {
  const [pendingCount, setPendingCount] = useState(0);
  const [isSyncing, setIsSyncing] = useState(false);

  const refreshCount = useCallback(async () => {
    try {
      const items = await getAllQueued();
      setPendingCount(items.length);
    } catch {
      // IndexedDB not available
    }
  }, []);

  useEffect(() => {
    refreshCount();
  }, [refreshCount]);

  const enqueue = useCallback(
    async (method: string, url: string, data?: unknown, headers?: Record<string, string>) => {
      const item: QueuedRequest = {
        id: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
        method,
        url,
        data,
        headers,
        timestamp: Date.now(),
      };
      await addToQueue(item);
      await refreshCount();
    },
    [refreshCount]
  );

  const sync = useCallback(async () => {
    if (!navigator.onLine || isSyncing) return;

    setIsSyncing(true);
    try {
      const items = await getAllQueued();

      for (const item of items) {
        try {
          const token = localStorage.getItem('token');
          await fetch(item.url, {
            method: item.method,
            headers: {
              'Content-Type': 'application/json',
              ...(token ? { Authorization: `Bearer ${token}` } : {}),
              ...item.headers,
            },
            body: item.data ? JSON.stringify(item.data) : undefined,
          });
          await removeFromQueue(item.id);
        } catch {
          // Keep in queue if still failing
          break;
        }
      }
    } finally {
      setIsSyncing(false);
      await refreshCount();
    }
  }, [isSyncing, refreshCount]);

  // Auto-sync when coming back online
  useEffect(() => {
    const handleOnline = () => {
      sync();
    };
    window.addEventListener('online', handleOnline);
    return () => window.removeEventListener('online', handleOnline);
  }, [sync]);

  const clear = useCallback(async () => {
    await clearQueue();
    await refreshCount();
  }, [refreshCount]);

  return { pendingCount, isSyncing, enqueue, sync, clear };
}
