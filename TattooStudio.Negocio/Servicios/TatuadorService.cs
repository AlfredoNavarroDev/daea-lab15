using AutoMapper;
using TattooStudio.Datos.Repositorios;
using TattooStudio.Entidades;
using TattooStudio.Entidades.Dtos;
using TattooStudio.Negocio.Excepciones;

namespace TattooStudio.Negocio.Servicios;

public class TatuadorService : ITatuadorService
{
    private readonly ITatuadorRepository _tatuadorRepository;
    private readonly IMapper _mapper;

    public TatuadorService(ITatuadorRepository tatuadorRepository, IMapper mapper)
    {
        _tatuadorRepository = tatuadorRepository;
        _mapper = mapper;
    }

    public async Task<List<TatuadorDto>> ObtenerTodosAsync(string? especialidad, bool? activo)
    {
        var tatuadores = await _tatuadorRepository.ObtenerTodosAsync(especialidad, activo);
        return _mapper.Map<List<TatuadorDto>>(tatuadores);
    }

    public async Task<TatuadorDto?> ObtenerPorIdAsync(int id)
    {
        var tatuador = await _tatuadorRepository.ObtenerPorIdConCitasAsync(id);
        return tatuador is null ? null : _mapper.Map<TatuadorDto>(tatuador);
    }

    public async Task<TatuadorDto> CrearAsync(TatuadorCrearDto dto)
    {
        var tatuador = _mapper.Map<Tatuador>(dto);
        tatuador.Activo = true;
        await _tatuadorRepository.CrearAsync(tatuador);
        return _mapper.Map<TatuadorDto>(tatuador);
    }

    public async Task<bool> ActualizarAsync(int id, TatuadorActualizarDto dto)
    {
        var tatuador = await _tatuadorRepository.ObtenerPorIdAsync(id);
        if (tatuador is null)
        {
            return false;
        }

        if (tatuador.Activo && !dto.Activo)
        {
            var tieneCitasActivas = await _tatuadorRepository.TieneCitasActivasAsync(id);
            if (tieneCitasActivas)
            {
                throw new ReglaNegocioException(
                    $"No se puede desactivar al tatuador '{tatuador.Nombre}' porque tiene citas Pendientes o Confirmadas asociadas.");
            }
        }

        _mapper.Map(dto, tatuador);
        await _tatuadorRepository.ActualizarAsync(tatuador);
        return true;
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var tatuador = await _tatuadorRepository.ObtenerPorIdAsync(id);
        if (tatuador is null)
        {
            return false;
        }

        var tieneCitasActivas = await _tatuadorRepository.TieneCitasActivasAsync(id);
        if (tieneCitasActivas)
        {
            throw new ReglaNegocioException(
                $"No se puede eliminar al tatuador '{tatuador.Nombre}' porque tiene citas Pendientes o Confirmadas asociadas.");
        }

        tatuador.Activo = false;
        await _tatuadorRepository.ActualizarAsync(tatuador);
        return true;
    }
}
