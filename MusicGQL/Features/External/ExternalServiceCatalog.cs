namespace MusicGQL.Features.External;

public record ExternalServiceModel(string Id, string Name, bool Enabled);

public static class ExternalServiceCatalog
{
    public static readonly IReadOnlyList<ExternalServiceModel> All =
    [
        new("musicbrainz", "MusicBrainz", true),
        new("spotify", "Spotify", true),
        new("apple-music", "Apple Music", true),
        new("youtube", "YouTube", true),
        new("tidal", "TIDAL", false),
        new("deezer", "Deezer", false),
        new("soundcloud", "SoundCloud", true),
        new("bandcamp", "Bandcamp", false),
        new("discogs", "Discogs", false)
    ];

    public static ExternalServiceModel Musicbrainz() => GetById("musicbrainz")!;
    public static ExternalServiceModel Spotify() => GetById("spotify")!;
    public static ExternalServiceModel Apple() => GetById("apple-music")!;
    public static ExternalServiceModel Youtube() => GetById("youtube")!;
    public static ExternalServiceModel Tidal() => GetById("tidal")!;
    public static ExternalServiceModel Deezer() => GetById("deezer")!;
    public static ExternalServiceModel Soundcloud() => GetById("soundcloud")!;
    public static ExternalServiceModel Bandcamp() => GetById("bandcamp")!;
    public static ExternalServiceModel Discogs() => GetById("discogs")!;
    public static ExternalServiceModel? GetById(string id) => All.FirstOrDefault(s => s.Id == id);
}