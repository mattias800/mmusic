using AutoMapper;
using MusicGQL.Db.Neo4j.ServerLibrary.MusicMetaData;

namespace MusicGQL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Hqub.MusicBrainz.Entities.Artist, Artist>();
        CreateMap<Hqub.MusicBrainz.Entities.Recording, Recording>();
        CreateMap<Hqub.MusicBrainz.Entities.Relation, Relation>();
        CreateMap<Hqub.MusicBrainz.Entities.Release, Release>();
        CreateMap<Hqub.MusicBrainz.Entities.ReleaseGroup, ReleaseGroup>();
    }
}
