using CafeBarrio.Application.Common.Interfaces;
using MUIS_CORE.Wrappers;
using MediatR;

namespace CafeBarrio.Application.Features.Sunat.Commands.ReprocesarSunat;

public class ReprocesarSunatHandler : IRequestHandler<ReprocesarSunatCommand, Result>
{
    private readonly ITransaccionRepository _transacciones;
    private readonly IUnitOfWork _uow;

    public ReprocesarSunatHandler(ITransaccionRepository transacciones, IUnitOfWork uow)
    {
        _transacciones = transacciones;
        _uow           = uow;
    }

    public async Task<Result> Handle(ReprocesarSunatCommand request, CancellationToken ct)
    {
        var tx = await _transacciones.GetByIdAsync(request.TransaccionId, ct);
        if (tx is null)
            return Result.Failure(new Error("Transaccion.NotFound",
                $"Transacción {request.TransaccionId} no encontrada."));

        if (tx.SunatEstado is not ("DeadLetter" or "NoEmitida"))
            return Result.Failure(new Error("Sunat.EstadoInvalido",
                $"Solo se pueden reprocesar transacciones DeadLetter o NoEmitida. Estado actual: {tx.SunatEstado}"));

        tx.SunatEstado   = "Pendiente";
        tx.SunatIntentos = 0;
        tx.SunatError    = null;

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
