namespace MusicGQL.Features.ServerLibrary.Artist.Sagas;

public record AddArtistToServerLibrarySagaEvents
{
    public record StartAddArtist(string ArtistMbId);

    public record FindArtistInMusicBrainz(string ArtistMbId);

    public record FoundArtistInMusicBrainz(
        string ArtistMbId,
        Hqub.MusicBrainz.Entities.Artist Artist,
        IEnumerable<Hqub.MusicBrainz.Entities.ReleaseGroup> ReleaseGroups
    );

    public record DidNotFindArtistInMusicBrainz(string ArtistMbId);
}
