using CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Tests.Integration.Base;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CafeBarrio.Tests.Integration.Features.Transacciones;

[Trait("Category", "Integration")]
public class TransaccionesIntegrationTests : IntegrationTestBase
{
    private readonly CreateTransaccionHandler _handler;

    private class DummySunatService : CafeBarrio.Application.Common.Interfaces.ISunatService
    {
        public Task<CafeBarrio.Application.Common.Interfaces.EmitirBoletaResult> EmitirBoletaAsync(CafeBarrio.Application.Common.Interfaces.EmitirBoletaRequest request, CancellationToken ct = default)
        {
            return Task.FromResult(new CafeBarrio.Application.Common.Interfaces.EmitirBoletaResult(true, "B001-1", "Dummy"));
        }
    }

    public TransaccionesIntegrationTests() : base()
    {
        var transaccionesRepo = new TransaccionRepository(Db);
        var productosRepo = new ProductoRepository(Db);
        var configRepo = new ConfiguracionNegocioRepository(Db);
        var uow = new UnitOfWork(Db);

        _handler = new CreateTransaccionHandler(transaccionesRepo, productosRepo, configRepo, uow, new DummySunatService());
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
            SeguimientoInventario = true, UnidadMedida = "tz", Activo = true, FechaCreacion = DateTime.UtcNow, FechaActualizacion = DateTime.UtcNow
        };
        Db.Productos.Add(producto);

        var config = new ConfiguracionNegocio { Sede = sede, TasaIGV = 0.18m, TasaIPM = 0m, FechaVigencia = DateTime.UtcNow, Activo = true };
        Db.ConfiguracionesNegocio.Add(config);

        await Db.SaveChangesAsync();

        var items = new List<CreateTransaccionItemDto> { new CreateTransaccionItemDto(producto.ProductoId, 2) };
        var command = new CreateTransaccionCommand(
            SedeId: sede.SedeId,
            MetodoPagoId: mp.MetodoPagoId,
            Items: items,
            ClienteId: cliente.ClienteId,
            Canal: "POS",
            TurnoId: turno.TurnoId,
            OperadorId: op.OperadorId
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);

        var prodUpdated = await Db.Productos.FindAsync(producto.ProductoId);
        prodUpdated.CantidadDisponible.Should().Be(8);
    }
}
