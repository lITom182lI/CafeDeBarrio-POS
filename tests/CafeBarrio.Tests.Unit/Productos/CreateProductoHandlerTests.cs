using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Productos.Commands.CreateProducto;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using MUIS_CORE.Wrappers;
using NSubstitute;
using Xunit;

namespace CafeBarrio.Tests.Unit.Productos;

public class CreateProductoHandlerTests
{
    private readonly IProductoRepository _productos = Substitute.For<IProductoRepository>();
    private readonly IUnitOfWork _uow               = Substitute.For<IUnitOfWork>();
    private readonly CreateProductoHandler _sut;

    public CreateProductoHandlerTests() => _sut = new CreateProductoHandler(_productos, _uow);

    private static CreateProductoCommand BuildCommand() =>
        new("Café Americano", null, 3m, 6m, 100m, 10m, "unidad", false, false, 1);

    [Fact]
    public async Task Handle_RepositoryFailure_PropagatesError()
    {
        _productos.AddAsync(Arg.Any<Producto>(), Arg.Any<CancellationToken>())
                  .Returns(Result<int>.Failure(new Error("Repo.Error", "Fallo al guardar.")));

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Repo.Error");
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsProductoId()
    {
        _productos.AddAsync(Arg.Any<Producto>(), Arg.Any<CancellationToken>())
                  .Returns(Result<int>.Success(5));
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }
}
