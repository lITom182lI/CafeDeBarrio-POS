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

    public CreateAnulacionHandler(
        ITransaccionRepository transacciones,
        IAnulacionRepository anulaciones,
        IOperadorRepository operadores,
        IProductoRepository productos,
        IUnitOfWork uow)
    {
        _transacciones = transacciones;
        _anulaciones   = anulaciones;
        _operadores    = operadores;
        _productos     = productos;
        _uow           = uow;
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

        var autorizador = await _operadores.GetByIdAsync(request.AutorizadorId, ct);
        if (autorizador is null)
            return Result<int>.Failure(new Error("Anulacion.AutorizadorNotFound",
                $"Operador autorizador {request.AutorizadorId} no encontrado."));

        if (request.MontoDevuelto > transaccion.Total)
            return Result<int>.Failure(new Error("Anulacion.MontoInvalido",
                "El monto devuelto no puede superar el total de la transacción."));

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
            TransaccionId          = request.TransaccionId,
            TipoAnulacion          = request.TipoAnulacion,
            Motivo                 = request.Motivo,
            MontoOriginal          = transaccion.Total,
            MontoDevuelto          = request.MontoDevuelto,
            MetodoDevolucion       = request.MetodoDevolucion,
            OperadorSolicitanteId  = request.OperadorSolicitanteId,
            AutorizadorId          = request.AutorizadorId,
            FechaHora              = DateTime.UtcNow,
            ImpactoInventario      = request.ImpactoInventario
        };

        var result = await _anulaciones.AddAsync(anulacion, ct);
        if (result.IsFailure) return Result<int>.Failure(result.Errors);

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(result.Value);
    }
}
