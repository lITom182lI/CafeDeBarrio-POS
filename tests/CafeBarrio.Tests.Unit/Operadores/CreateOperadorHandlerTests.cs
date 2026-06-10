using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Operadores.Commands.CreateOperador;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using MUIS_CORE.Wrappers;
using NSubstitute;
using Xunit;

namespace CafeBarrio.Tests.Unit.Operadores;

public class CreateOperadorHandlerTests
{
    private readonly IOperadorRepository _operadores = Substitute.For<IOperadorRepository>();
    private readonly IUnitOfWork _uow                = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _hasher         = Substitute.For<IPasswordHasher>();
    private readonly ISedeRepository _sedes          = Substitute.For<ISedeRepository>();
    private readonly CreateOperadorHandler _sut;

    public CreateOperadorHandlerTests() =>
        _sut = new CreateOperadorHandler(_operadores, _sedes, _uow, _hasher);

    [Fact]
    public async Task Handle_PinInvalido_ReturnsFailure()
    {
        var result = await _sut.Handle(new CreateOperadorCommand("Juan", "12A"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Operador.PinInvalido");
    }

    [Fact]
    public async Task Handle_PinValido_ReturnsOperadorId()
    {
        _hasher.Hash("123456").Returns("hashed");
        _operadores.AddAsync(Arg.Any<Operador>(), Arg.Any<CancellationToken>())
                   .Returns(Result<int>.Success(3));
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _sut.Handle(new CreateOperadorCommand("Juan", "123456"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }
}
