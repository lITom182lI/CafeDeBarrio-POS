using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Commands.AnularTransaccion;

public class AnularTransaccionHandler : IRequestHandler<AnularTransaccionCommand, Result>
{
    private readonly ITransaccionRepository _transacciones;
    private readonly IAnulacionRepository _anulaciones;
    private readonly IOperadorRepository _operadores;
    private readonly IProductoRepository _productos;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AnularTransaccionHandler(
        ITransaccionRepository transacciones,
        IAnulacionRepository anulaciones,
        IOperadorRepository operadores,
        IProductoRepository productos,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _transacciones = transacciones;
        _anulaciones   = anulaciones;
        _operadores    = operadores;
        _productos     = productos;
        _uow           = uow;
        _currentUser   = currentUser;
    }

    public async Task<Result> Handle(AnularTransaccionCommand request, CancellationToken ct)
    {
        var transaccion = await _transacciones.GetWithDetallesAndAnulacionAsync(request.TransaccionId, ct);
        if (transaccion is null)
            return Result.Failure(new Error("Transaccion.NotFound",
                $"Transacción {request.TransaccionId} no encontrada."));

        if (transaccion.Anulacion is not null)
            return Result.Failure(new Error("Transaccion.AlreadyCancelled",
                "La transacción ya se encuentra anulada."));

        if (transaccion.SunatEstado == "Emitida")
            return Result.Failure(new Error("Transaccion.RequiereNotaCredito",
                "La transacción fue emitida ante SUNAT. Para anularla se debe emitir una Nota de Crédito."));

        var solicitante = await _operadores.GetByIdAsync(request.OperadorSolicitanteId, ct);
        if (solicitante is null)
            return Result.Failure(new Error("Anulacion.OperadorNotFound",
                $"Operador solicitante {request.OperadorSolicitanteId} no encontrado."));

        if (_currentUser.UserId is null)
            return Result.Failure(new Error("Auth.Unauthenticated",
                "No se pudo determinar la identidad del autorizador."));

        var autorizadorId = await _operadores.GetOperadorIdByUsuarioIdAsync(_currentUser.UserId.Value, ct);
        if (autorizadorId is null)
            return Result.Failure(new Error("Anulacion.AutorizadorSinOperador",
                "El usuario Admin no tiene un Operador vinculado. Vincúlalo para autorizar anulaciones."));

        await _uow.BeginTransactionAsync(ct);
        try
        {
            foreach (var detalle in transaccion.Detalles)
            {
                var producto = await _productos.GetByIdAsync(detalle.ProductoId, ct);
                if (producto is not null && producto.SeguimientoInventario)
                    producto.CantidadDisponible += detalle.Cantidad;
            }

            var anulacion = new Anulacion
            {
                TransaccionId         = request.TransaccionId,
                TipoAnulacion         = "Total",
                Motivo                = request.Motivo,
                MontoOriginal         = transaccion.Total,
                MontoDevuelto         = transaccion.Total,
                MetodoDevolucion      = request.MetodoDevolucion,
                OperadorSolicitanteId = request.OperadorSolicitanteId,
                AutorizadorId         = autorizadorId.Value,
                FechaHora             = DateTime.UtcNow,
                ImpactoInventario     = true
            };

            await _anulaciones.AddAsync(anulacion, ct);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            return Result.Success();
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
