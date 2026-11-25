using AutoMapper;
using TattooStudio.Datos.Repositorios;
using TattooStudio.Entidades;
using TattooStudio.Entidades.Dtos;
using TattooStudio.Negocio.Excepciones;

namespace TattooStudio.Negocio.Servicios;

public class CitaTatuajeService : ICitaTatuajeService
{
    private readonly ICitaTatuajeRepository _citaRepository;
    private readonly ITatuadorRepository _tatuadorRepository;
    private readonly IMapper _mapper;

    public CitaTatuajeService(
        ICitaTatuajeRepository citaRepository,
        ITatuadorRepository tatuadorRepository,
        IMapper mapper)
    {
        _citaRepository = citaRepository;
        _tatuadorRepository = tatuadorRepository;
        _mapper = mapper;
    }

    public async Task<List<CitaTatuajeDto>> ObtenerTodosAsync(int? tatuadorId, string? estado)
    {
        var citas = await _citaRepository.ObtenerTodosAsync(tatuadorId, estado);
        return _mapper.Map<List<CitaTatuajeDto>>(citas);
    }

    public async Task<List<CitaTatuajeDto>> ObtenerPorTatuadorAsync(int tatuadorId)
    {
        var citas = await _citaRepository.ObtenerPorTatuadorAsync(tatuadorId);
        return _mapper.Map<List<CitaTatuajeDto>>(citas);
    }

    public async Task<CitaTatuajeDto?> ObtenerPorIdAsync(int id)
    {
        var cita = await _citaRepository.ObtenerPorIdAsync(id);
        return cita is null ? null : _mapper.Map<CitaTatuajeDto>(cita);
    }

    public async Task<CitaTatuajeDto?> CrearAsync(CitaTatuajeCrearDto dto)
    {
        var tatuador = await _tatuadorRepository.ObtenerPorIdAsync(dto.TatuadorId);
        if (tatuador is null)
        {
            return null;
        }

        await ValidarSolapamientoAsync(dto.TatuadorId, dto.FechaHora, dto.EstimacionHoras, excluirCitaId: null);

        var cita = _mapper.Map<CitaTatuaje>(dto);
        cita.Estado = "Pendiente";
        await _citaRepository.CrearAsync(cita);

        cita = await _citaRepository.ObtenerPorIdAsync(cita.Id);
        return _mapper.Map<CitaTatuajeDto>(cita);
    }

    public async Task<bool?> ActualizarAsync(int id, CitaTatuajeActualizarDto dto)
    {
        var cita = await _citaRepository.ObtenerPorIdConTatuadorAsync(id);
        if (cita is null)
        {
            return null;
        }

        var tatuador = await _tatuadorRepository.ObtenerPorIdAsync(dto.TatuadorId);
        if (tatuador is null)
        {
            return null;
        }

        if (dto.Estado != "Cancelada")
        {
            await ValidarSolapamientoAsync(dto.TatuadorId, dto.FechaHora, dto.EstimacionHoras, excluirCitaId: id);
        }

        _mapper.Map(dto, cita);
        await _citaRepository.ActualizarAsync(cita);
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var cita = await _citaRepository.ObtenerPorIdAsync(id);
        if (cita is null)
        {
            return false;
        }

        await _citaRepository.EliminarAsync(cita);
        return true;
    }

    private async Task ValidarSolapamientoAsync(int tatuadorId, DateTime fechaHora, decimal estimacionHoras, int? excluirCitaId)
    {
        var nuevoInicio = fechaHora;
        var nuevoFin = fechaHora.AddHours((double)estimacionHoras);

        var citasActivas = await _citaRepository.ObtenerActivasPorTatuadorAsync(tatuadorId, excluirCitaId);

        var seSolapa = citasActivas.Any(c =>
        {
            var inicioExistente = c.FechaHora;
            var finExistente = c.FechaHora.AddHours((double)c.EstimacionHoras);
            return nuevoInicio < finExistente && inicioExistente < nuevoFin;
        });

        if (seSolapa)
        {
            throw new ReglaNegocioException(
                "El tatuador ya tiene otra cita activa que se solapa con el horario indicado.");
        }
    }
}
