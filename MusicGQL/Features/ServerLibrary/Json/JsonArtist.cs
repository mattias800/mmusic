namespace MusicGQL.Features.ServerLibrary.Json;

public class JsonArtist
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? SortName { get; set; }
    public List<JsonArtistAlias>? Aliases { get; set; }
    public long? MonthlyListeners { get; set; }
    public List<JsonTopTrack>? TopTracks { get; set; }
    public JsonArtistPhotos? Photos { get; set; }
    public JsonArtistServiceConnections? Connections { get; set; }
    public List<JsonArtistAppearance>? AlsoAppearsOn { get; set; }
}

public class JsonArtistAlias
{
    public string? Name { get; set; }
    public string? SortName { get; set; }
    public string? BeginDate { get; set; }
    public string? EndDate { get; set; }
    public string? Type { get; set; }
    public string? Locale { get; set; }
}

public class JsonTopTrack
{
    public string Title { get; set; } = string.Empty;
    public string? ReleaseTitle { get; set; }
    public int? TrackLength { get; set; }
    public string? CoverArt { get; set; }
    public string? ReleaseFolderName { get; set; }
    public int? TrackNumber { get; set; }
    public long? PlayCount { get; set; }
    public double? RankScore { get; set; }
    public string? RankSource { get; set; } // "lf", "sp_popularity", or "lf+sp"
}

public class JsonArtistPhotos
{
    public List<string>? Backgrounds { get; set; }
    public List<string>? Thumbs { get; set; }
    public List<string>? Banners { get; set; }
    public List<string>? Logos { get; set; }
}

public class JsonArtistServiceConnections
{
    public string? MusicBrainzArtistId { get; set; }
    // Legacy single Spotify ID for backward compatibility
    public string? SpotifyId { get; set; }
    // New: support multiple linked Spotify artist identities
    public List<JsonSpotifyArtistIdentity>? SpotifyIds { get; set; }

    public string? YoutubeChannelUrl { get; set; }
    public string? YoutubeMusicUrl { get; set; }
    public string? AppleMusicArtistId { get; set; }
    public string? DeezerArtistId { get; set; }
    public string? TidalArtistId { get; set; }
    public string? SoundcloudUrl { get; set; }
    public string? BandcampUrl { get; set; }
    public string? DiscogsUrl { get; set; }
}

public class JsonSpotifyArtistIdentity
{
    public string Id { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Source { get; set; } // e.g., "musicbrainz" or "search"
    public bool? Verified { get; set; }
    public long? Followers { get; set; }
    public string? AddedAt { get; set; }
}

/// <summary>
/// Represents an appearance by this artist on a release where they are not the primary artist
/// </summary>
public class JsonArtistAppearance
{
    /// <summary>
    /// Title of the release/album
    /// </summary>
    public string ReleaseTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of release (Album, EP, Single)
    /// </summary>
    public string ReleaseType { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the primary artist for this release
    /// </summary>
    public string PrimaryArtistName { get; set; } = string.Empty;
    
    /// <summary>
    /// MusicBrainz ID of the primary artist
    /// </summary>
    public string? PrimaryArtistMusicBrainzId { get; set; }
    
    /// <summary>
    /// MusicBrainz ID of the release group
    /// </summary>
    public string? MusicBrainzReleaseGroupId { get; set; }
    
    /// <summary>
    /// First release date of the release group
    /// </summary>
    public string? FirstReleaseDate { get; set; }
    
    /// <summary>
    /// Year of first release
    /// </summary>
    public string? FirstReleaseYear { get; set; }
    
    /// <summary>
    /// Role of this artist on the release (e.g., "Featured Artist", "Producer", "Composer")
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// Cover art URL if available
    /// </summary>
    public string? CoverArt { get; set; }
}