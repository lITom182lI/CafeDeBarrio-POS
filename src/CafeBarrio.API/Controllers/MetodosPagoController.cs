using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/metodos-pago")]
[AllowAnonymous]
public class MetodosPagoController : ControllerBase
{
    private readonly CafeBarrioDbContext _context;

    public MetodosPagoController(CafeBarrioDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMetodosPago()
    {
        var lista = await _context.Set<MetodoPago>()
            .Select(m => new { m.MetodoPagoId, m.Nombre })
            .ToListAsync();
        return Ok(lista);
    }
}
