using CafeBarrio.Application.Events;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Anulaciones.Commands.CreateAnulacion;

public class CreateAnulacionHandler : IRequestHandler<CreateAnulacionCommand, Result<int>>
{
    private readonly ITransaccionRepository _transacciones;
    private readonly IAnulacionRepository _anulaciones;
    private readonly IOperadorRepository _operadores;
    private readonly IProductoRepository _productos;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;
    private readonly ICurrentUserService _currentUser;

    public CreateAnulacionHandler(
        ITransaccionRepository transacciones,
        IAnulacionRepository anulaciones,
        IOperadorRepository operadores,
        IProductoRepository productos,
        IUnitOfWork uow,
        IPublisher publisher,
        ICurrentUserService currentUser)
    {
        _transacciones = transacciones;
        _anulaciones   = anulaciones;
        _operadores    = operadores;
        _productos     = productos;
        _uow           = uow;
        _publisher     = publisher;
        _currentUser   = currentUser;
    }

    public async Task<Result<int>> Handle(CreateAnulacionCommand request, CancellationToken ct)
    {
        var transaccion = await _transacciones.GetWithDetallesAndAnulacionAsync(request.TransaccionId, ct);
        if (transaccion is null)
            return Result<int>.Failure(new Error("Anulacion.TransaccionNotFound",
                $"Transacción {request.TransaccionId} no encontrada."));

        if (transaccion.Anulacion is not null)
            return Result<int>.Failure(new Error("Anulacion.YaAnulada",
                $"La transacción {request.TransaccionId} ya fue anulada."));

        var solicitante = await _operadores.GetByIdAsync(request.OperadorSolicitanteId, ct);
        if (solicitante is null)
            return Result<int>.Failure(new Error("Anulacion.OperadorNotFound",
                $"Operador solicitante {request.OperadorSolicitanteId} no encontrado."));

        // Autorizador derivado del JWT — Admin debe tener Operador vinculado
        if (_currentUser.UserId is null)
            return Result<int>.Failure(new Error("Auth.Unauthenticated",
                "No se pudo determinar la identidad del autorizador."));

        var autorizadorId = await _operadores.GetOperadorIdByUsuarioIdAsync(_currentUser.UserId.Value, ct);
        if (autorizadorId is null)
            return Result<int>.Failure(new Error("Anulacion.AutorizadorSinOperador",
                "El usuario Admin no tiene un Operador vinculado. Vincúlalo para autorizar anulaciones."));

        if (request.MontoDevuelto > transaccion.Total)
            return Result<int>.Failure(new Error("Anulacion.MontoInvalido",
                "El monto devuelto no puede superar el total de la transacción."));

        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (request.ImpactoInventario)
            {
                foreach (var detalle in transaccion.Detalles)
                {
                    var producto = await _productos.GetByIdAsync(detalle.ProductoId, ct);
                    if (producto is not null && producto.SeguimientoInventario)
                        producto.CantidadDisponible += detalle.Cantidad;
                }
            }

            var anulacion = new Anulacion
            {
                TransaccionId         = request.TransaccionId,
                TipoAnulacion         = request.TipoAnulacion,
                Motivo                = request.Motivo,
                MontoOriginal         = transaccion.Total,
                MontoDevuelto         = request.MontoDevuelto,
                MetodoDevolucion      = request.MetodoDevolucion,
                OperadorSolicitanteId = request.OperadorSolicitanteId,
                AutorizadorId         = autorizadorId.Value,
                FechaHora             = DateTime.UtcNow,
                ImpactoInventario     = request.ImpactoInventario
            };

            await _anulaciones.AddAsync(anulacion, ct);

            try
            {
                await _uow.SaveChangesAsync(ct);
            }
            catch (CafeBarrio.Application.Common.Exceptions.ConcurrencyException)
            {
                await _uow.RollbackAsync(ct);
                return Result<int>.Failure(new Error("Anulacion.ConcurrencyConflict",
                    "Conflicto de inventario al anular. Reintenta."));
            }

            await _uow.CommitAsync(ct);

            await _publisher.Publish(new AnulacionAprobadaEvent(
                anulacion.AnulacionId, anulacion.TransaccionId,
                anulacion.MontoDevuelto, anulacion.TipoAnulacion), ct);

            return Result<int>.Success(anulacion.AnulacionId);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
