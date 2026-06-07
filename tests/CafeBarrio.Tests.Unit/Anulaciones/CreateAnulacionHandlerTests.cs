using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Anulaciones.Commands.CreateAnulacion;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;
using MUIS_CORE.Wrappers;
using System.Threading;
using System.Threading.Tasks;

namespace CafeBarrio.Tests.Unit.Anulaciones;

public class CreateAnulacionHandlerTests
{
    private readonly ITransaccionRepository _transacciones;
    private readonly IAnulacionRepository _anulaciones;
    private readonly IOperadorRepository _operadores;
    private readonly IProductoRepository _productos;
    private readonly IUnitOfWork _uow;
    private readonly CreateAnulacionHandler _sut;

    public CreateAnulacionHandlerTests()
    {
        _transacciones = Substitute.For<ITransaccionRepository>();
        _anulaciones   = Substitute.For<IAnulacionRepository>();
        _operadores    = Substitute.For<IOperadorRepository>();
        _productos     = Substitute.For<IProductoRepository>();
        _uow           = Substitute.For<IUnitOfWork>();

        _sut = new CreateAnulacionHandler(
            _transacciones,
            _anulaciones,
            _operadores,
            _productos,
            _uow);
    }

    [Fact]
    public async Task Handle_TransaccionNotFound_ReturnsFailure()
    {
        var command = new CreateAnulacionCommand(1, "Total", "Error", 100, null, 1, 2, true);
        _transacciones.GetWithDetallesAndAnulacionAsync(1, Arg.Any<CancellationToken>())
            .Returns((Transaccion?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Anulacion.TransaccionNotFound");
    }

    [Fact]
    public async Task Handle_YaAnulada_ReturnsFailure()
    {
        var command = new CreateAnulacionCommand(1, "Total", "Error", 100, null, 1, 2, true);
        var transaccion = new Transaccion { TransaccionId = 1, Total = 100, Anulacion = new Anulacion() };
        _transacciones.GetWithDetallesAndAnulacionAsync(1, Arg.Any<CancellationToken>())
            .Returns(transaccion);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Anulacion.YaAnulada");
    }

    [Fact]
    public async Task Handle_MontoInvalido_ReturnsFailure()
    {
        var command = new CreateAnulacionCommand(1, "Parcial", "Error", 150, null, 1, 2, true);
        var transaccion = new Transaccion { TransaccionId = 1, Total = 100 };
        _transacciones.GetWithDetallesAndAnulacionAsync(1, Arg.Any<CancellationToken>())
            .Returns(transaccion);
        _operadores.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Operador());
        _operadores.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(new Operador());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Anulacion.MontoInvalido");
    }

    [Fact]
    public async Task Handle_OperadorNotFound_ReturnsFailure()
    {
        var command = new CreateAnulacionCommand(1, "Total", "Error", 100, null, 1, 2, true);
        var transaccion = new Transaccion { TransaccionId = 1, Total = 100 };
        _transacciones.GetWithDetallesAndAnulacionAsync(1, Arg.Any<CancellationToken>())
            .Returns(transaccion);
        _operadores.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Operador?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Anulacion.OperadorNotFound");
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        var command = new CreateAnulacionCommand(1, "Total", "Error", 100, null, 1, 2, false);
        var transaccion = new Transaccion { TransaccionId = 1, Total = 100 };
        var operador    = new Operador { OperadorId = 1, Nombre = "Juan" };
        var autorizador = new Operador { OperadorId = 2, Nombre = "Jefe" };

        _transacciones.GetWithDetallesAndAnulacionAsync(1, Arg.Any<CancellationToken>())
            .Returns(transaccion);
        _operadores.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(operador);
        _operadores.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(autorizador);
        _anulaciones.AddAsync(Arg.Any<Anulacion>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(99)));
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }
}
