using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Productos.Commands.DeleteProducto;

public record DeleteProductoCommand(int ProductoId) : IRequest<Result>;
