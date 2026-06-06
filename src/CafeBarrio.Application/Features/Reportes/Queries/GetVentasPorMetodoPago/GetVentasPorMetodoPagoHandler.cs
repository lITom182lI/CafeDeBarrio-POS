using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorMetodoPago;

public class GetVentasPorMetodoPagoHandler : IRequestHandler<GetVentasPorMetodoPagoQuery, Result<IReadOnlyList<VentasPorMetodoPagoDto>>>
{
    private readonly IReportesRepository _repo;
    public GetVentasPorMetodoPagoHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<VentasPorMetodoPagoDto>>> Handle(GetVentasPorMetodoPagoQuery request, CancellationToken ct)
    {
        var (desde, hasta) = PeriodoHelper.Calcular(request.Periodo);
        var dtos = await _repo.GetVentasPorMetodoPagoAsync(request.SedeId, desde, hasta, ct);
        return Result<IReadOnlyList<VentasPorMetodoPagoDto>>.Success(dtos);
    }
}
