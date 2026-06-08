# ADR-002: Arquitectura Offline-First — Cliente POS

**Estado:** ACEPTADO
**Fecha:** 2026-06-05
**Referencia:** Suposición S1 resuelta (Domain Charter v1.0.0-SEALED-2026-06-05)

## Contexto
S1 confirmado: el barista necesita registrar ventas cuando el servidor no responde.
El cliente POS no puede depender de conectividad continua.

## Decisión
Arquitectura offline-first con IndexedDB (via biblioteca idb) local + sincronización en background:
1. Toda venta se guarda primero en IndexedDB local (DB: 'cafe-barrio-pos', store: pendingTransactions)
2. Se intenta sincronizar con la API inmediatamente
3. Si falla, un SyncService reintenta cada 30 segundos
4. El catálogo de productos se cachea localmente al inicio (stores: catalogProductos, catalogCategorias, catalogMetodosPago)

## Consecuencias
- Ventas nunca se pierden por falta de conectividad
- El barista opera normalmente sin internet
- La API es el sistema de registro oficial — IndexedDB es cola de entrada
- Eventual consistency: el Dashboard refleja ventas con delay máx. 30s offline

## Inmutabilidad
Inmutable. Si se requiere cambio, crear ADR-003.
