using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TattooStudio.Entidades.Dtos;
using TattooStudio.Negocio.Excepciones;
using TattooStudio.Negocio.Servicios;

namespace TattooStudio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TatuadoresController : ControllerBase
{
    private readonly ITatuadorService _tatuadorService;
    private readonly ICitaTatuajeService _citaService;

    public TatuadoresController(ITatuadorService tatuadorService, ICitaTatuajeService citaService)
    {
        _tatuadorService = tatuadorService;
        _citaService = citaService;
    }

    // GET api/tatuadores?especialidad=Realismo&activo=true
    [HttpGet]
    public async Task<ActionResult<List<TatuadorDto>>> ObtenerTodos([FromQuery] string? especialidad, [FromQuery] bool? activo)
    {
        var tatuadores = await _tatuadorService.ObtenerTodosAsync(especialidad, activo);
        return Ok(tatuadores);
    }

    // GET api/tatuadores/5
    [HttpGet("{id:int}", Name = "ObtenerTatuadorPorId")]
    public async Task<ActionResult<TatuadorDto>> ObtenerPorId(int id)
    {
        var tatuador = await _tatuadorService.ObtenerPorIdAsync(id);
        if (tatuador is null)
        {
            return NotFound();
        }

        return Ok(tatuador);
    }

    // GET api/tatuadores/5/citas
    [HttpGet("{tatuadorId:int}/citas")]
    public async Task<ActionResult<List<CitaTatuajeDto>>> ObtenerCitasDelTatuador(int tatuadorId)
    {
        var tatuador = await _tatuadorService.ObtenerPorIdAsync(tatuadorId);
        if (tatuador is null)
        {
            return NotFound();
        }

        var citas = await _citaService.ObtenerPorTatuadorAsync(tatuadorId);
        return Ok(citas);
    }

    // POST api/tatuadores
    [HttpPost]
    [Authorize(Roles = "Recepcionista")]
    public async Task<ActionResult<TatuadorDto>> Crear([FromBody] TatuadorCrearDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var creado = await _tatuadorService.CrearAsync(dto);
        return CreatedAtRoute("ObtenerTatuadorPorId", new { id = creado.Id }, creado);
    }

    // PUT api/tatuadores/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Recepcionista")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] TatuadorActualizarDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var actualizado = await _tatuadorService.ActualizarAsync(id, dto);
            if (!actualizado)
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

    // DELETE api/tatuadores/5 (baja logica: Activo = false)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Recepcionista")]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            var desactivado = await _tatuadorService.DesactivarAsync(id);
            if (!desactivado)
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
}
