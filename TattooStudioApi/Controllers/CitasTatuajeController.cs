using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TattooStudio.Entidades.Dtos;
using TattooStudio.Negocio.Excepciones;
using TattooStudio.Negocio.Servicios;

namespace TattooStudioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CitasTatuajeController : ControllerBase
{
    private readonly ICitaTatuajeService _citaService;

    public CitasTatuajeController(ICitaTatuajeService citaService)
    {
        _citaService = citaService;
    }

    // GET api/citastatuaje?tatuadorId=1&estado=Pendiente
    [HttpGet]
    public async Task<ActionResult<List<CitaTatuajeDto>>> ObtenerTodos([FromQuery] int? tatuadorId, [FromQuery] string? estado)
    {
        var citas = await _citaService.ObtenerTodosAsync(tatuadorId, estado);
        return Ok(citas);
    }

    // GET api/citastatuaje/5
    [HttpGet("{id:int}", Name = "ObtenerCitaPorId")]
    public async Task<ActionResult<CitaTatuajeDto>> ObtenerPorId(int id)
    {
        var cita = await _citaService.ObtenerPorIdAsync(id);
        if (cita is null)
        {
            return NotFound();
        }

        return Ok(cita);
    }

    // POST api/citastatuaje
    [HttpPost]
    [Authorize(Roles = "Recepcionista")]
    public async Task<ActionResult<CitaTatuajeDto>> Crear([FromBody] CitaTatuajeCrearDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var creada = await _citaService.CrearAsync(dto);
            if (creada is null)
            {
                return NotFound(new { mensaje = "El tatuador indicado no existe." });
            }

            return CreatedAtRoute("ObtenerCitaPorId", new { id = creada.Id }, creada);
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    // PUT api/citastatuaje/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Recepcionista,Tatuador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CitaTatuajeActualizarDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var resultado = await _citaService.ActualizarAsync(id, dto);
            if (resultado is null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ReglaNegocioException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    // DELETE api/citastatuaje/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Recepcionista")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminada = await _citaService.EliminarAsync(id);
        if (!eliminada)
        {
            return NotFound();
        }

        return NoContent();
    }
}
