using CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Tests.Integration.Base;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MediatR;
using CafeBarrio.Application.Common.Interfaces;
using MUIS_CORE.Wrappers;
using MUIS_CORE.Pagination;

namespace CafeBarrio.Tests.Integration.Features.Transacciones;

[Trait("Category", "Integration")]
public class TransaccionesIntegrationTests : IntegrationTestBase
{
    private readonly CreateTransaccionHandler _handler;



    private class DummyPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
    }

    private class DummyIdempotencyRepo : IIdempotencyRecordRepository
    {
        public Task<IdempotencyRecord?> GetByKeyAsync(string idempotencyKey, CancellationToken ct = default) => Task.FromResult<IdempotencyRecord?>(null);
        public Task<IdempotencyRecord?> GetByIdAsync(int id, CancellationToken ct = default) => Task.FromResult<IdempotencyRecord?>(null);
        public Task<MUIS_CORE.Pagination.PagedResult<IdempotencyRecord>> GetPagedAsync(MUIS_CORE.Pagination.PaginationRequest request, CancellationToken ct = default) => null!;
        public Task<Result<int>> AddAsync(IdempotencyRecord entity, CancellationToken ct = default) => Task.FromResult(Result<int>.Success(1));
        public Task<Result> UpdateAsync(IdempotencyRecord entity, CancellationToken ct = default) => Task.FromResult(Result.Success());
        public Task<Result> DeleteAsync(int id, CancellationToken ct = default) => Task.FromResult(Result.Success());
    }

    private class DummyCurrentUserService : ICurrentUserService
    {
        public string? Email => "test@test.com";
        public int? SedeId => null;
        public int? UserId => 1;
    }

    public TransaccionesIntegrationTests() : base()
    {
        var transaccionesRepo = new TransaccionRepository(Db);
        var productosRepo = new ProductoRepository(Db);
        var configRepo = new ConfiguracionNegocioRepository(Db);
        var uow = new UnitOfWork(Db);
        var pub = new DummyPublisher();
        var idemp = new DummyIdempotencyRepo();
        var currentUser = new DummyCurrentUserService();
        var turnosRepo = new TurnoRepository(Db);

        _handler = new CreateTransaccionHandler(transaccionesRepo, productosRepo, configRepo, uow, pub, idemp, currentUser, turnosRepo);
    }

    [Fact]
    public async Task CreateTransaccion_ShouldPersist_AndDiscountStock()
    {
        // Arrange
        var sede = new Sede { Nombre = "Sede 1", Direccion = "Dir", Distrito = "D", Ciudad = "C", Activa = true };
        Db.Sedes.Add(sede);

        var op = new Operador { Nombre = "Op 1", PinHash = "hash", Activo = true, Sede = sede };
        Db.Operadores.Add(op);

        var turno = new Turno { Operador = op, Sede = sede, FechaApertura = DateTime.UtcNow, Estado = "Abierto", MontoApertura = 100 };
        Db.Turnos.Add(turno);

        var cat = new CategoriaCafe { Codigo = "C1", Nombre = "Cafe", Activa = true };
        Db.CategoriasCafe.Add(cat);

        var mp = new MetodoPago { Nombre = "Efectivo", Activo = true };
        Db.MetodosPago.Add(mp);

        var tipoCliente = new TipoCliente { Nombre = "Regular" };
        Db.TiposCliente.Add(tipoCliente);

        var cliente = new Cliente { TipoCliente = tipoCliente, Nombre = "C1", Apellido = "A1", Email = "e@e.com", Activo = true, FechaRegistro = new DateOnly(2026,1,1) };
        Db.Clientes.Add(cliente);

        var producto = new Producto 
        { 
            Nombre = "Espresso", Costo = 2, Precio = 5, CantidadDisponible = 10, Categoria = cat,
            SeguimientoInventario = true, UnidadMedida = "tz", Activo = true
        };
        Db.Productos.Add(producto);

        var config = new ConfiguracionNegocio { Sede = sede, TasaIGV = 0.18m, TasaIPM = 0m, FechaVigencia = DateTime.UtcNow, Activo = true };
        Db.ConfiguracionesNegocio.Add(config);

        await Db.SaveChangesAsync();

        var command = new CreateTransaccionCommand(
            SedeId: sede.SedeId,
            MetodoPagoId: mp.MetodoPagoId,
            Items: new[] { new CreateTransaccionItemDto(producto.ProductoId, 2) })
        {
            IdempotencyKey = "integration-test-key-1",
            ClienteId = cliente.ClienteId,
            Canal = "POS",
            TurnoId = turno.TurnoId,
            OperadorId = op.OperadorId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);

        var prodUpdated = await Db.Productos.FindAsync(producto.ProductoId);
        prodUpdated.Should().NotBeNull();
        prodUpdated!.CantidadDisponible.Should().Be(8);
    }

    [Fact]
    public async Task CreateTransaccion_SameIdempotencyKey_ReturnsSameNonZeroId()
    {
        // Arrange — handler con IdempotencyRecordRepository real
        var idempRepo = new IdempotencyRecordRepository(Db);
        var handlerReal = new CreateTransaccionHandler(
            new TransaccionRepository(Db),
            new ProductoRepository(Db),
            new ConfiguracionNegocioRepository(Db),
            new UnitOfWork(Db),
            new DummyPublisher(),
            idempRepo,
            new DummyCurrentUserService(),
            new TurnoRepository(Db));

        var sede     = new Sede { Nombre = "Sede Idemp", Direccion = "D", Distrito = "D", Ciudad = "C", Activa = true };
        var cat      = new CategoriaCafe { Codigo = "CI", Nombre = "Cafe", Activa = true };
        var mp       = new MetodoPago { Nombre = "Efectivo", Activo = true };
        var tc       = new TipoCliente { Nombre = "Regular" };
        Db.Sedes.Add(sede); Db.CategoriasCafe.Add(cat); Db.MetodosPago.Add(mp); Db.TiposCliente.Add(tc);
        await Db.SaveChangesAsync();

        var op     = new Operador { Nombre = "Op", PinHash = "h", Activo = true, Sede = sede };
        var turno  = new Turno { Operador = op, Sede = sede, FechaApertura = DateTime.UtcNow, Estado = "Abierto", MontoApertura = 100 };
        var cli    = new Cliente { TipoCliente = tc, Nombre = "X", Apellido = "Y", Email = "x@x.com", Activo = true, FechaRegistro = new DateOnly(2026, 1, 1) };
        var prod   = new Producto { Nombre = "Espresso", Costo = 2, Precio = 5, CantidadDisponible = 20, Categoria = cat, SeguimientoInventario = true, UnidadMedida = "tz", Activo = true };
        var config = new ConfiguracionNegocio { Sede = sede, TasaIGV = 0.18m, TasaIPM = 0m, FechaVigencia = DateTime.UtcNow, Activo = true };
        Db.Operadores.Add(op); Db.Turnos.Add(turno); Db.Clientes.Add(cli); Db.Productos.Add(prod); Db.ConfiguracionesNegocio.Add(config);
        await Db.SaveChangesAsync();

        var command = new CreateTransaccionCommand(
            SedeId: sede.SedeId, MetodoPagoId: mp.MetodoPagoId,
            Items: new[] { new CreateTransaccionItemDto(prod.ProductoId, 1) })
        {
            IdempotencyKey = "idemp-retry-test-001",
            ClienteId      = cli.ClienteId,
            Canal          = "POS",
            TurnoId        = turno.TurnoId,
            OperadorId     = op.OperadorId
        };

        // Act — misma key dos veces
        var result1 = await handlerReal.Handle(command, CancellationToken.None);
        var result2 = await handlerReal.Handle(command, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be(result2.Value);
        result1.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateTransaccion_InsufficientStock_ReturnsFailure()
    {
        // Arrange
        var sede   = new Sede { Nombre = "Sede Stock", Direccion = "D", Distrito = "D", Ciudad = "C", Activa = true };
        var cat    = new CategoriaCafe { Codigo = "CS", Nombre = "Cafe", Activa = true };
        var mp     = new MetodoPago { Nombre = "Efectivo", Activo = true };
        var tc     = new TipoCliente { Nombre = "Regular" };
        Db.Sedes.Add(sede); Db.CategoriasCafe.Add(cat); Db.MetodosPago.Add(mp); Db.TiposCliente.Add(tc);
        await Db.SaveChangesAsync();

        var op     = new Operador { Nombre = "Op", PinHash = "h", Activo = true, Sede = sede };
        var turno  = new Turno { Operador = op, Sede = sede, FechaApertura = DateTime.UtcNow, Estado = "Abierto", MontoApertura = 100 };
        var cli    = new Cliente { TipoCliente = tc, Nombre = "X", Apellido = "Y", Email = "s@s.com", Activo = true, FechaRegistro = new DateOnly(2026, 1, 1) };
        var prod   = new Producto { Nombre = "Espresso", Costo = 2, Precio = 5, CantidadDisponible = 1, Categoria = cat, SeguimientoInventario = true, UnidadMedida = "tz", Activo = true };
        var config = new ConfiguracionNegocio { Sede = sede, TasaIGV = 0.18m, TasaIPM = 0m, FechaVigencia = DateTime.UtcNow, Activo = true };
        Db.Operadores.Add(op); Db.Turnos.Add(turno); Db.Clientes.Add(cli); Db.Productos.Add(prod); Db.ConfiguracionesNegocio.Add(config);
        await Db.SaveChangesAsync();

        var command = new CreateTransaccionCommand(
            SedeId: sede.SedeId, MetodoPagoId: mp.MetodoPagoId,
            Items: new[] { new CreateTransaccionItemDto(prod.ProductoId, 5) })  // stock = 1
        {
            IdempotencyKey = "stock-insuf-test-001",
            ClienteId      = cli.ClienteId,
            Canal          = "POS",
            TurnoId        = turno.TurnoId,
            OperadorId     = op.OperadorId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Stock.Insuficiente");
    }
}
