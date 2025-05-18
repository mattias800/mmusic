using AutoMapper;
using MusicGQL.Db.Models.ServerLibrary.MusicMetaData;
using Tag = MusicGQL.Db.Models.ServerLibrary.MusicMetaData.Tag;

namespace MusicGQL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Hqub.MusicBrainz.Entities.Alias, Alias>();
        CreateMap<Hqub.MusicBrainz.Entities.Area, Area>();
        CreateMap<Hqub.MusicBrainz.Entities.Artist, Artist>();
        CreateMap<Hqub.MusicBrainz.Entities.CoverArtArchive, CoverArtArchive>();
        CreateMap<Hqub.MusicBrainz.Entities.Disc, Disc>();
        CreateMap<Hqub.MusicBrainz.Entities.Genre, Genre>();
        CreateMap<Hqub.MusicBrainz.Entities.Label, Label>();
        CreateMap<Hqub.MusicBrainz.Entities.LabelInfo, LabelInfo>();
        CreateMap<Hqub.MusicBrainz.Entities.LifeSpan, LifeSpan>();
        CreateMap<Hqub.MusicBrainz.Entities.Medium, Medium>();
        CreateMap<Hqub.MusicBrainz.Entities.NameCredit, NameCredit>();
        CreateMap<Hqub.MusicBrainz.Entities.Rating, Rating>();
        CreateMap<Hqub.MusicBrainz.Entities.Recording, Recording>();
        CreateMap<Hqub.MusicBrainz.Entities.Relation, Relation>();
        CreateMap<Hqub.MusicBrainz.Entities.Release, Release>();
        CreateMap<Hqub.MusicBrainz.Entities.ReleaseGroup, ReleaseGroup>();
        CreateMap<Hqub.MusicBrainz.Entities.Tag, Tag>();
        CreateMap<Hqub.MusicBrainz.Entities.TextRepresentation, TextRepresentation>();
        CreateMap<Hqub.MusicBrainz.Entities.Track, Track>();
        CreateMap<Hqub.MusicBrainz.Entities.Url, Url>();
        CreateMap<Hqub.MusicBrainz.Entities.Work, Work>();
    }
}
