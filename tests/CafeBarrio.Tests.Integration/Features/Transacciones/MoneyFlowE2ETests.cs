using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Anulaciones.Commands.CreateAnulacion;
using CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;
using CafeBarrio.Application.Features.Turnos.Commands.AbrirTurno;
using CafeBarrio.Application.Features.Turnos.Commands.CerrarTurno;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Tests.Integration.Base;
using FluentAssertions;
using MediatR;
using MUIS_CORE.Pagination;
using MUIS_CORE.Wrappers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CafeBarrio.Tests.Integration.Features.Transacciones;

[Trait("Category", "Integration")]
public class MoneyFlowE2ETests : IntegrationTestBase
{
    private class DummyPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
        public Task Publish<T>(T notification, CancellationToken ct = default)
            where T : INotification => Task.CompletedTask;
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

    private class DummyCurrentUser : ICurrentUserService
    {
        public string? Email  => null;
        public int?    SedeId => null;
        public int?    UserId => null;
    }

    private class CurrentUserWithId : ICurrentUserService
    {
        public CurrentUserWithId(int userId) => UserId = userId;
        public string? Email  => "admin@test.com";
        public int?    SedeId => null;
        public int?    UserId { get; }
    }

    /// <summary>
    /// Ciclo completo de caja:
    ///   Turno1 — abrir(100) → venta efectivo(1×5.00) → venta dividida(2×5.00; ef=4) → cerrar
    ///   Turno2 — abrir(50)  → anulación cross-turno(venta1 en ef) → cerrar
    ///
    /// Verifica:
    ///   - IGV inclusivo: Total=5.00, Subtotal=4.24, Impuesto=0.76
    ///   - Arqueo Turno1: VentasEf=9.00, Saldo=109.00
    ///   - Anulación cross-turno cae en Turno2, NO en Turno1
    ///   - Arqueo Turno2: Anulaciones=5.00, Saldo=45.00
    /// </summary>
    [Fact]
    public async Task MoneyFlow_AbrirVentaAnularCerrar_CorrectoArqueoIGV()
    {
        // ── Seed ─────────────────────────────────────────────────────────────
        var sede = new Sede { Nombre = "E2E", Direccion = "D", Distrito = "D", Ciudad = "C", Activa = true };
        Db.Sedes.Add(sede);

        var autorizadorUsuario = new Usuario
        {
            Email        = "autorizador-e2e@test.com",
            PasswordHash = "h",
            Rol          = "Administrador",
            Activo       = true
        };
        Db.Usuarios.Add(autorizadorUsuario);
        await Db.SaveChangesAsync();

        var operador = new Operador
        {
            SedeId    = sede.SedeId,
            Nombre    = "OpE2E",
            PinHash   = "h",
            Activo    = true,
            UsuarioId = autorizadorUsuario.UsuarioId
        };
        Db.Operadores.Add(operador);

        var cat      = new CategoriaCafe { Codigo = "E2E", Nombre = "Cafe", Activa = true };
        var efectivo = new MetodoPago { Nombre = "Efectivo-E2E", Activo = true, EsEfectivo = true };
        var tarjeta  = new MetodoPago { Nombre = "Tarjeta-E2E",  Activo = true, EsEfectivo = false };
        var config   = new ConfiguracionNegocio
        {
            SedeId        = sede.SedeId,
            TasaIGV       = 0.18m,
            TasaIPM       = 0m,
            FechaVigencia = DateTime.UtcNow,
            Activo        = true
        };
        Db.CategoriasCafe.Add(cat);
        Db.MetodosPago.Add(efectivo);
        Db.MetodosPago.Add(tarjeta);
        Db.ConfiguracionesNegocio.Add(config);
        await Db.SaveChangesAsync();

        var producto = new Producto
        {
            Nombre                = "Espresso-E2E",
            Costo                 = 2m,
            Precio                = 5m,
            CantidadDisponible    = 100,
            Categoria             = cat,
            SeguimientoInventario = true,
            UnidadMedida          = "tz",
            Activo                = true
        };
        Db.Productos.Add(producto);
        await Db.SaveChangesAsync();

        // ── Handlers ─────────────────────────────────────────────────────────
        var uow       = new UnitOfWork(Db);
        var publisher = new DummyPublisher();
        var turnoRepo = new TurnoRepository(Db);

        var abrirHandler  = new AbrirTurnoHandler(turnoRepo, uow, new DummyCurrentUser());
        var cerrarHandler = new CerrarTurnoHandler(turnoRepo, uow);
        var ventaHandler  = new CreateTransaccionHandler(
            new TransaccionRepository(Db),
            new ProductoRepository(Db),
            new ConfiguracionNegocioRepository(Db),
            uow, publisher,
            new DummyIdempotencyRepo(),
            new DummyCurrentUser(),
            turnoRepo);
        var anulacionHandler = new CreateAnulacionHandler(
            new TransaccionRepository(Db),
            new AnulacionRepository(Db),
            new OperadorRepository(Db),
            new ProductoRepository(Db),
            uow, publisher,
            new CurrentUserWithId(autorizadorUsuario.UsuarioId));

        // ── 1. Abrir Turno 1 (apertura = S/100) ──────────────────────────────
        var abrir1 = await abrirHandler.Handle(
            new AbrirTurnoCommand(sede.SedeId, operador.OperadorId, 100m),
            CancellationToken.None);
        abrir1.IsSuccess.Should().BeTrue("debería poder abrir el primer turno");
        var turnoId1 = abrir1.Value;

        // ── 2. Venta simple: 1 × S/5.00 (efectivo) ──────────────────────────
        var venta1 = await ventaHandler.Handle(
            new CreateTransaccionCommand(
                sede.SedeId, efectivo.MetodoPagoId,
                new[] { new CreateTransaccionItemDto(producto.ProductoId, 1) })
            {
                IdempotencyKey = "e2e-v1",
                TurnoId        = turnoId1,
                OperadorId     = operador.OperadorId,
                Canal          = "POS"
            }, CancellationToken.None);
        venta1.IsSuccess.Should().BeTrue();
        var transaccionId1 = venta1.Value;

        // Assert IGV inclusivo: Total=5.00, base=4.24, igv=0.76
        var t1 = await Db.Transacciones.FindAsync(transaccionId1);
        t1!.Total.Should().Be(5.00m);
        t1.Subtotal.Should().Be(4.24m);
        t1.Impuesto.Should().Be(0.76m);

        // ── 3. Venta dividida: 2 × S/5.00; efectivo=4.00, tarjeta=6.00 ──────
        var venta2 = await ventaHandler.Handle(
            new CreateTransaccionCommand(
                sede.SedeId, efectivo.MetodoPagoId,
                new[] { new CreateTransaccionItemDto(producto.ProductoId, 2) })
            {
                IdempotencyKey         = "e2e-v2",
                TurnoId                = turnoId1,
                OperadorId             = operador.OperadorId,
                Canal                  = "POS",
                MetodoPagoSecundarioId = tarjeta.MetodoPagoId,
                MontoMetodoPrimario    = 4.00m
            }, CancellationToken.None);
        venta2.IsSuccess.Should().BeTrue();

        // ── 4. Cerrar Turno 1 ─────────────────────────────────────────────────
        // VentasEfectivo = 5.00 (venta1) + 4.00 (porción ef de venta2) = 9.00
        // SaldoEsperado  = 100 + 9.00 = 109.00
        var cerrar1 = await cerrarHandler.Handle(
            new CerrarTurnoCommand(turnoId1, 109.00m, "Cierre T1"),
            CancellationToken.None);
        cerrar1.IsSuccess.Should().BeTrue();
        cerrar1.Value!.TotalEfectivoSistema.Should().Be(109.00m);
        cerrar1.Value!.Diferencia.Should().Be(0m);

        // ── 5. Abrir Turno 2 (apertura = S/50) ──────────────────────────────
        var abrir2 = await abrirHandler.Handle(
            new AbrirTurnoCommand(sede.SedeId, operador.OperadorId, 50m),
            CancellationToken.None);
        abrir2.IsSuccess.Should().BeTrue();
        var turnoId2 = abrir2.Value;

        // ── 6. Anulación cross-turno: venta1 devuelta en efectivo durante Turno 2
        // La devolución ocurre DESPUÉS del cierre de Turno1, así que cae en Turno2
        var anulacion = await anulacionHandler.Handle(
            new CreateAnulacionCommand(
                TransaccionId:         transaccionId1,
                TipoAnulacion:         "Devolucion",
                Motivo:                "Test cross-turno",
                MontoDevuelto:         5.00m,
                MetodoDevolucion:      "Efectivo",
                OperadorSolicitanteId: operador.OperadorId,
                ImpactoInventario:     true),
            CancellationToken.None);
        anulacion.IsSuccess.Should().BeTrue();

        // ── 7. Arqueo Turno 2: devolución registrada aquí ─────────────────────
        // SaldoEsperado = 50 (apertura) - 5.00 (devuelto) = 45.00
        var resumen2 = await turnoRepo.GetResumenEfectivoAsync(turnoId2, CancellationToken.None);
        resumen2.TotalVentasEfectivo.Should().Be(0m);
        resumen2.TotalAnulacionesEfectivo.Should().Be(5.00m);
        resumen2.SaldoEsperado.Should().Be(45.00m);

        // ── 8. Arqueo Turno 1: anulación NO aparece (ocurrió post-cierre) ─────
        var resumen1 = await turnoRepo.GetResumenEfectivoAsync(turnoId1, CancellationToken.None);
        resumen1.TotalAnulacionesEfectivo.Should().Be(0m);
        resumen1.SaldoEsperado.Should().Be(109.00m);

        // ── 9. Cerrar Turno 2 ─────────────────────────────────────────────────
        var cerrar2 = await cerrarHandler.Handle(
            new CerrarTurnoCommand(turnoId2, 45.00m, "Cierre T2 cross-turno"),
            CancellationToken.None);
        cerrar2.IsSuccess.Should().BeTrue();
        cerrar2.Value!.TotalEfectivoSistema.Should().Be(45.00m);
        cerrar2.Value!.Diferencia.Should().Be(0m);
    }
}
