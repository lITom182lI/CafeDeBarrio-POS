using CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;
using CafeBarrio.Domain.Common;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Tests.Integration.Base;
using FluentAssertions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CafeBarrio.Application.Common.Interfaces;
using MUIS_CORE.Wrappers;
using MUIS_CORE.Pagination;

namespace CafeBarrio.Tests.Integration.Features.Transacciones;

[Trait("Category", "Integration")]
public class ArqueoIntegrationTests : IntegrationTestBase
{
    private readonly CreateTransaccionHandler _handler;
    private readonly TurnoRepository _turnoRepo;

    private class DummyPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
            where TNotification : INotification => Task.CompletedTask;
    }

    private class DummyIdempotencyRepo : IIdempotencyRecordRepository
    {
        public Task<IdempotencyRecord?> GetByKeyAsync(string key, CancellationToken ct = default)
            => Task.FromResult<IdempotencyRecord?>(null);
        public Task<IdempotencyRecord?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult<IdempotencyRecord?>(null);
        public Task<PagedResult<IdempotencyRecord>> GetPagedAsync(PaginationRequest req, CancellationToken ct = default)
            => null!;
        public Task<Result<int>> AddAsync(IdempotencyRecord entity, CancellationToken ct = default)
            => Task.FromResult(Result<int>.Success(1));
        public Task<Result> UpdateAsync(IdempotencyRecord entity, CancellationToken ct = default)
            => Task.FromResult(Result.Success());
        public Task<Result> DeleteAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Result.Success());
    }

    private class DummyCurrentUserService : ICurrentUserService
    {
        public string? Email  => "test@test.com";
        public int?    SedeId => null;
        public int?    UserId => 1;
    }

    public ArqueoIntegrationTests() : base()
    {
        _turnoRepo = new TurnoRepository(Db);
        _handler   = new CreateTransaccionHandler(
            new TransaccionRepository(Db),
            new ProductoRepository(Db),
            new ConfiguracionNegocioRepository(Db),
            new UnitOfWork(Db),
            new DummyPublisher(),
            new DummyIdempotencyRepo(),
            new DummyCurrentUserService(),
            _turnoRepo);
    }

    private async Task<(Sede sede, Turno turno, MetodoPago efectivo, Producto producto)>
        SeedBaseAsync(decimal montoApertura, string idPrefix)
    {
        var sede = new Sede { Nombre = $"Sede-{idPrefix}", Direccion = "D", Distrito = "D", Ciudad = "C", Activa = true };
        Db.Sedes.Add(sede);
        await Db.SaveChangesAsync();

        var op = new Operador { SedeId = sede.SedeId, Nombre = "Op", PinHash = "h", Activo = true };
        Db.Operadores.Add(op);
        await Db.SaveChangesAsync();

        var cat     = new CategoriaCafe { Codigo = $"CAT-{idPrefix}", Nombre = "Cafe", Activa = true };
        var efectivo = new MetodoPago  { Nombre = $"Efectivo-{idPrefix}", Activo = true, EsEfectivo = true };
        var config  = new ConfiguracionNegocio
        {
            SedeId = sede.SedeId, TasaIGV = 0.18m, TasaIPM = 0m,
            FechaVigencia = DateTime.UtcNow, Activo = true
        };
        var turno = new Turno
        {
            SedeId = sede.SedeId, OperadorId = op.OperadorId,
            FechaApertura = DateTime.UtcNow, Estado = "Abierto",
            MontoApertura = montoApertura
        };

        Db.CategoriasCafe.Add(cat);
        Db.MetodosPago.Add(efectivo);
        Db.ConfiguracionesNegocio.Add(config);
        Db.Turnos.Add(turno);
        await Db.SaveChangesAsync();

        // Precio = 5m (IGV-inclusivo): Total = 5.00, base = Round(5/1.18) = 4.24, igv = 0.76
        var producto = new Producto
        {
            Nombre = "Espresso", Costo = 2m, Precio = 5m,
            CantidadDisponible = 100, Categoria = cat,
            SeguimientoInventario = true, UnidadMedida = "tz", Activo = true
        };
        Db.Productos.Add(producto);
        await Db.SaveChangesAsync();

        return (sede, turno, efectivo, producto);
    }

    // Apertura = 100, 1 venta efectivo (Total=5.00) → SaldoEsperado = 105.00
    [Fact]
    public async Task Arqueo_VentaEfectivoSimple_SaldoEsperadoCorrecto()
    {
        var (sede, turno, efectivo, producto) = await SeedBaseAsync(100m, "V1");

        var cmd = new CreateTransaccionCommand(
            sede.SedeId, efectivo.MetodoPagoId,
            new[] { new CreateTransaccionItemDto(producto.ProductoId, 1) })
        {
            IdempotencyKey = "arqueo-simple-001",
            Canal  = "POS",
            TurnoId = turno.TurnoId
        };

        (await _handler.Handle(cmd, CancellationToken.None)).IsSuccess.Should().BeTrue();

        var resumen = await _turnoRepo.GetResumenEfectivoAsync(turno.TurnoId, CancellationToken.None);

        resumen.TotalVentasEfectivo.Should().Be(5.00m);
        resumen.TotalAnulacionesEfectivo.Should().Be(0m);
        resumen.SaldoEsperado.Should().Be(105.00m);
    }

    // Apertura = 200, pago dividido: efectivo=5.00, tarjeta=6.80 (Total=11.80)
    // → TotalVentasEfectivo debe ser 5.00 (MontoMetodoPrimario), no 11.80
    [Fact]
    public async Task Arqueo_PagoDividido_SoloPortionEfectivoCuentaEnVentas()
    {
        var (sede, turno, efectivo, producto) = await SeedBaseAsync(200m, "V2");

        var tarjeta = new MetodoPago { Nombre = "Tarjeta-V2", Activo = true, EsEfectivo = false };
        Db.MetodosPago.Add(tarjeta);
        await Db.SaveChangesAsync();

        // Cantidad=2: Subtotal=10, IGV=Round(10*0.18)=1.80, Total=11.80
        var cmd = new CreateTransaccionCommand(
            sede.SedeId, efectivo.MetodoPagoId,
            new[] { new CreateTransaccionItemDto(producto.ProductoId, 2) })
        {
            IdempotencyKey          = "arqueo-split-002",
            Canal                   = "POS",
            TurnoId                 = turno.TurnoId,
            MetodoPagoSecundarioId  = tarjeta.MetodoPagoId,
            MontoMetodoPrimario     = 5.00m   // efectivo; tarjeta cubre 6.80
        };

        (await _handler.Handle(cmd, CancellationToken.None)).IsSuccess.Should().BeTrue();

        var resumen = await _turnoRepo.GetResumenEfectivoAsync(turno.TurnoId, CancellationToken.None);

        resumen.TotalVentasEfectivo.Should().Be(5.00m);   // no 11.80
        resumen.SaldoEsperado.Should().Be(200m + 5.00m);  // 205.00
    }

    // Apertura = 150, 1 venta efectivo (Total=5.00) + entrada S/50 + salida S/30
    // → SaldoEsperado = 150 + 5.00 + 50 - 30 = 175.00
    [Fact]
    public async Task Arqueo_ConMovimientosCaja_SaldoEsperadoIncluye()
    {
        var (sede, turno, efectivo, producto) = await SeedBaseAsync(150m, "V3");

        var cmd = new CreateTransaccionCommand(
            sede.SedeId, efectivo.MetodoPagoId,
            new[] { new CreateTransaccionItemDto(producto.ProductoId, 1) })
        {
            IdempotencyKey = "arqueo-mov-003",
            Canal   = "POS",
            TurnoId = turno.TurnoId
        };
        (await _handler.Handle(cmd, CancellationToken.None)).IsSuccess.Should().BeTrue();

        Db.MovimientosCaja.Add(new MovimientoCaja
        {
            TurnoId        = turno.TurnoId,
            TipoMovimiento = TipoMovimiento.Entrada,
            Monto          = 50m,
            Motivo         = "Fondo extra",
            FechaHora      = DateTime.UtcNow
        });
        Db.MovimientosCaja.Add(new MovimientoCaja
        {
            TurnoId        = turno.TurnoId,
            TipoMovimiento = TipoMovimiento.Salida,
            Monto          = 30m,
            Motivo         = "Gasto",
            FechaHora      = DateTime.UtcNow
        });
        await Db.SaveChangesAsync();

        var resumen = await _turnoRepo.GetResumenEfectivoAsync(turno.TurnoId, CancellationToken.None);

        resumen.TotalVentasEfectivo.Should().Be(5.00m);
        resumen.TotalEntradasCaja.Should().Be(50m);
        resumen.TotalSalidasCaja.Should().Be(30m);
        resumen.SaldoEsperado.Should().Be(175.00m);
    }
}
