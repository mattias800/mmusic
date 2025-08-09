using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ServerLibrary;

public record TrackStatistics([property: GraphQLIgnore] CachedTrackStatistics Model)
{
    public long PlayCount() => Model.JsonTrackStatistics.PlayCount;

    public long Listeners() => Model.JsonTrackStatistics.Listeners;
}
