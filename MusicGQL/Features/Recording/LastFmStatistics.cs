namespace MusicGQL.Features.Recording;

public record LastFmStatistics([property: GraphQLIgnore] Hqub.Lastfm.Entities.Statistics Model)
{
    public long Listeners => Model.Listeners;
    public long PlayCount => Model.PlayCount;
}
