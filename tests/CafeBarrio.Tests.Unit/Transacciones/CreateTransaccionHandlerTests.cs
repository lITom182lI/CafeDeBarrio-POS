using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using MUIS_CORE.Wrappers;
using NSubstitute;
using Xunit;
using MediatR;

namespace CafeBarrio.Tests.Unit.Transacciones;

public class CreateTransaccionHandlerTests
{
    private readonly ITransaccionRepository _transacciones = Substitute.For<ITransaccionRepository>();
    private readonly IProductoRepository _productos         = Substitute.For<IProductoRepository>();
    private readonly IConfiguracionNegocioRepository _conf  = Substitute.For<IConfiguracionNegocioRepository>();
    private readonly IUnitOfWork _uow                       = Substitute.For<IUnitOfWork>();
    private readonly IPublisher _publisher                  = Substitute.For<IPublisher>();
    private readonly IIdempotencyRecordRepository _idemp    = Substitute.For<IIdempotencyRecordRepository>();
    private readonly ICurrentUserService _currentUser       = Substitute.For<ICurrentUserService>();
    private readonly ITurnoRepository _turnos               = Substitute.For<ITurnoRepository>();
    private readonly CreateTransaccionHandler _sut;

    public CreateTransaccionHandlerTests()
    {
        _uow.ExecuteInTransactionAsync(
                Arg.Any<Func<CancellationToken, Task<int>>>(),
                Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<int>>>()(CancellationToken.None));

        _sut = new CreateTransaccionHandler(_transacciones, _productos, _conf, _uow, _publisher, _idemp, _currentUser, _turnos);
    }

    private static CreateTransaccionCommand BuildCommand(int productoId = 1, int cantidad = 2)
        => new(SedeId: 1, MetodoPagoId: 1, Items: new[] { new CreateTransaccionItemDto(productoId, cantidad) })
        {
            IdempotencyKey = "test-key-123"
        };

    private static ConfiguracionNegocio BuildConfig() => new()
    {
        TasaIGV = 0.16m,
        TasaIPM = 0.02m,
        Activo   = true
    };

    [Fact]
    public async Task Handle_ProductoNotFound_ReturnsFailure()
    {
        _conf.GetActivaBySedeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(BuildConfig());
        _productos.GetByIdAsync(1, Arg.Any<CancellationToken>())
                  .Returns((Producto?)null);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Producto.NotFound");
    }

    [Fact]
    public async Task Handle_StockInsuficiente_ReturnsFailure()
    {
        _conf.GetActivaBySedeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(BuildConfig());
        _productos.GetByIdAsync(1, Arg.Any<CancellationToken>())
                  .Returns(new Producto
                  {
                      ProductoId             = 1,
                      Precio                 = 10m,
                      SeguimientoInventario  = true,
                      CantidadDisponible     = 1m
                  });

        var result = await _sut.Handle(BuildCommand(cantidad: 5), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Stock.Insuficiente");
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        _conf.GetActivaBySedeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(BuildConfig());
        _productos.GetByIdAsync(1, Arg.Any<CancellationToken>())
                  .Returns(new Producto
                  {
                      ProductoId             = 1,
                      Precio                 = 10m,
                      SeguimientoInventario  = false,
                      CantidadDisponible     = 100m
                  });
        _transacciones.AddAsync(Arg.Any<Transaccion>(), Arg.Any<CancellationToken>())
                      .Returns(Result<int>.Success(77));
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConfiguracionNoEncontrada_ReturnsFailure()
    {
        _conf.GetActivaBySedeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns((ConfiguracionNegocio?)null);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Configuracion.NotFound");
    }
}
