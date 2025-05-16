using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Hqub.MusicBrainz.Entities.Artist, MusicGQL.Db.Models.ServerLibrary.Artist>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.SortName, opt => opt.MapFrom(src => src.SortName))
            .ForMember(dest => dest.Disambiguation, opt => opt.MapFrom(src => src.Disambiguation))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area))
            .ForMember(dest => dest.BeginArea, opt => opt.MapFrom(src => src.BeginArea))
            .ForMember(dest => dest.EndArea, opt => opt.MapFrom(src => src.EndArea));

        CreateMap<Hqub.MusicBrainz.Entities.Area, MusicGQL.Db.Models.ServerLibrary.Area>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Disambiguation, opt => opt.MapFrom(src => src.Disambiguation))
            .ForMember(dest => dest.IsoCodes, opt => opt.MapFrom(src => src.IsoCodes));
    }
}
