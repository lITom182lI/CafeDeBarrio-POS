using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Productos.Commands.UpdateProducto;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using MUIS_CORE.Wrappers;
using NSubstitute;
using Xunit;

namespace CafeBarrio.Tests.Unit.Productos;

public class UpdateProductoHandlerTests
{
    private readonly IProductoRepository _productos = Substitute.For<IProductoRepository>();
    private readonly IUnitOfWork _uow               = Substitute.For<IUnitOfWork>();
    private readonly UpdateProductoHandler _sut;

    public UpdateProductoHandlerTests() => _sut = new UpdateProductoHandler(_productos, _uow);

    private static UpdateProductoCommand BuildCommand(int id = 1) =>
        new(id, "Café Latte", null, 4m, 8m, 50m, 5m, "unidad", false, false, 1, true);

    [Fact]
    public async Task Handle_ProductoNotFound_ReturnsFailure()
    {
        _productos.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Producto?)null);

        var result = await _sut.Handle(BuildCommand(99), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Producto.NotFound");
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAndReturnsSuccess()
    {
        var producto = new Producto { ProductoId = 1, Nombre = "Viejo" };
        _productos.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(producto);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(BuildCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        producto.Nombre.Should().Be("Café Latte");
    }
}
