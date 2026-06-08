using System;
using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Queries.GetVentasResumen;
using CafeBarrio.Application.Features.Reportes.Dtos;
using CafeBarrio.Domain.Entities;
using FluentAssertions;
using MUIS_CORE.Wrappers;
using NSubstitute;
using Xunit;

namespace CafeBarrio.Tests.Unit.Reportes;

public class GetVentasResumenHandlerTests
{
    private readonly IReportesRepository _repo = Substitute.For<IReportesRepository>();
    private readonly GetVentasResumenHandler _sut;

    public GetVentasResumenHandlerTests() => _sut = new GetVentasResumenHandler(_repo);

    [Fact]
    public async Task Handle_PeriodoDia_RetornaResumenDelRepo()
    {
        var esperado = new VentasResumenDto(1500m, 10, 150m, DateTime.UtcNow.Date, DateTime.UtcNow);
        _repo.GetVentasResumenAsync(
                Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
             .Returns(esperado);

        var result = await _sut.Handle(new GetVentasResumenQuery(1, "dia"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalVentas.Should().Be(1500m);
        result.Value.NumTransacciones.Should().Be(10);
    }
}
