using AutoMapper;
using TattooStudio.Entidades;
using TattooStudio.Entidades.Dtos;

namespace TattooStudio.Negocio;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Tatuador, TatuadorDto>()
            .ForMember(dest => dest.CantidadCitas, opt => opt.MapFrom(src => src.CitaTatuajes.Count));

        CreateMap<TatuadorCrearDto, Tatuador>();

        CreateMap<TatuadorActualizarDto, Tatuador>();

        CreateMap<CitaTatuaje, CitaTatuajeDto>()
            .ForMember(dest => dest.TatuadorNombre, opt => opt.MapFrom(src => src.Tatuador.Nombre));

        CreateMap<CitaTatuajeCrearDto, CitaTatuaje>();

        CreateMap<CitaTatuajeActualizarDto, CitaTatuaje>();
    }
}
