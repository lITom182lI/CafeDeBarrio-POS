using System.Collections.Generic;
using System.Linq;

namespace MUIS_CORE.Wrappers;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "El valor proporcionado es nulo.");
    public static readonly Error Unauthorized = new("Error.Unauthorized", "No autorizado para realizar esta operación.");
    public static readonly Error NotFound = new("Error.NotFound", "El recurso solicitado no fue encontrado.");

    // Agrupa múltiples mensajes de validación en un Error compuesto.
    public static Error Validation(IEnumerable<string> messages)
        => new("Validation.Failed", string.Join(" | ", messages));
}
