using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary;

public record TrackStatistics([property: GraphQLIgnore] JsonTrackStatistics Model)
{
    public long PlayCount() => Model.PlayCount;

    public long Listeners() => Model.Listeners;
}
