using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetStockBajo;

public class GetStockBajoHandler : IRequestHandler<GetStockBajoQuery, Result<IReadOnlyList<StockBajoDto>>>
{
    private readonly IReportesRepository _repo;
    public GetStockBajoHandler(IReportesRepository repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<StockBajoDto>>> Handle(GetStockBajoQuery request, CancellationToken ct)
    {
        var dtos = await _repo.GetStockBajoAsync(ct);
        return Result<IReadOnlyList<StockBajoDto>>.Success(dtos);
    }
}
