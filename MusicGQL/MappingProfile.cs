using AutoMapper;
using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Features.ServerLibrary.Common.Db;
using MusicGQL.Features.ServerLibrary.Recording.Db;
using MusicGQL.Features.ServerLibrary.Release.Db;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;

namespace MusicGQL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Hqub.MusicBrainz.Entities.Artist, DbArtist>();
        CreateMap<Hqub.MusicBrainz.Entities.Recording, DbRecording>();
        CreateMap<Hqub.MusicBrainz.Entities.Relation, DbRelation>()
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url.Resource));
        CreateMap<Hqub.MusicBrainz.Entities.Release, DbRelease>();
        CreateMap<Hqub.MusicBrainz.Entities.ReleaseGroup, DbReleaseGroup>();
    }
}
