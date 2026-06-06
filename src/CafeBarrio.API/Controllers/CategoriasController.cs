using CafeBarrio.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CategoriasController : ControllerBase
{
    private readonly CafeBarrioDbContext _db;
    public CategoriasController(CafeBarrioDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var cats = await _db.CategoriasCafe
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .Select(c => new { c.CategoriaId, c.Codigo, c.Nombre })
            .ToListAsync(ct);
        return Ok(cats);
    }
}
