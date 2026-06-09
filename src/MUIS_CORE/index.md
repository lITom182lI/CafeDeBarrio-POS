# MUIS_CORE — Librería de Contratos y Primitivos Compartidos
## Core Router & Master Index

**Versión:** 1.1
**Fecha:** Junio 2026
**Rol en el Ecosistema:** Librería base reutilizable. Proporciona los contratos, wrappers y behaviors que MUIS_BACKEND prescribe como estándares — aquí están implementados como código C# real.
**Aplica a:** Todo proyecto .NET del ecosistema MUIS que use MediatR + Clean Architecture.

> **Punto de entrada del ecosistema:** Clasificar el sistema en [MUIS_CLASIFICACION](../MUIS_CLASIFICACION/index.md) antes de activar este módulo. La tabla de activación por tipología (abajo) indica qué componentes usar según el resultado del cuestionario.

> **Relación con MUIS_BACKEND:** MUIS_BACKEND define el *qué* (Result Pattern, Repository Pattern, Validation Pipeline). MUIS_CORE provee el *cómo* en código compilable y reutilizable.

---

## Activación por Tipología

| Componente | Tipo 1 | Tipo 2 | Tipo 3 | Tipo 4 |
|---|---|---|---|---|
| `Result<T>` / `Result` | ✅ | ✅ | ✅ | ✅ |
| `Error` + `IResult` | ✅ | ✅ | ✅ | ✅ |
| `DomainException` | ✅ | ✅ | ✅ | ✅ |
| `ValidationBehavior` | ⚠️ opcional | ✅ | ✅ | ✅ |
| `IRepository<T>` (Offset) | ✅ | ✅ | ✅ | ⚠️ |
| `KeysetPagedResult<T>` | ❌ | ✅ | ✅ | ✅ |
| `IRepository<T, TId>` (Keyset) | ❌ | ✅ | ✅ | ✅ |

---

## Estructura del Módulo

```
MUIS_CORE/
├── Wrappers/
│   ├── Error.cs              — Registro de error con código + mensaje. Error.Validation() para grupos.
│   ├── IResult.cs            — Interfaz marcadora con static abstract Failure() — sin reflexión.
│   └── Result.cs             — Result + Result<T> con Map(), Bind() y conversiones implícitas.
├── Behaviors/
│   └── ValidationBehavior.cs — MediatR pipeline: valida TRequest antes del handler.
├── Interfaces/
│   └── IRepository.cs        — Contrato genérico CRUD + paginación para repositorios.
├── Pagination/
│   ├── PaginationRequest.cs  — OffsetPaginationRequest + KeysetPaginationRequest.
│   └── PagedResult.cs        — PagedResult<T> (Offset) + KeysetPagedResult<T> (Keyset sin COUNT).
├── Exceptions/
│   ├── DomainException.cs    — Base abstracta con ErrorCode.
│   ├── NotFoundException.cs  — Entidad no encontrada por Id.
│   └── UnauthorizedException.cs — Acción no autorizada.
└── DependencyInjection.cs    — AddMuisCore(): registra MediatR + ValidationBehavior opcional.
```

---

## Guía de uso rápido

### Registrar en el proyecto host
```csharp
// Program.cs — MediatR siempre se registra. Validación es opt-in.
builder.Services.AddMuisCore(
    applicationAssembly: typeof(MiHandler).Assembly,
    enableValidation: true);
```

### Retornar Result desde un Handler
```csharp
public async Task<Result<ClienteDto>> Handle(GetClienteQuery q, CancellationToken ct)
{
    var cliente = await _repo.GetByIdAsync(q.Id, ct);
    if (cliente is null)
        return Error.NotFound;    // Conversión implícita

    return _mapper.Map<ClienteDto>(cliente);  // Conversión implícita desde T
}
```

### Encadenar operaciones con Map y Bind
```csharp
var result = await _handler.Handle(query, ct);
var dto = result
    .Map(cliente => _mapper.Map<ClienteDto>(cliente))
    .Bind(dto => dto.Email.Contains("@")
        ? Result<ClienteDto>.Success(dto)
        : Error.Validation(["Email inválido"]));
```
