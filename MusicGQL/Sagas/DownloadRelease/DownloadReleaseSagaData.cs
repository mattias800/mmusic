using Hqub.MusicBrainz.Entities;
using Rebus.Sagas;

namespace MusicGQL.Sagas.DownloadRelease;

public class DownloadReleaseSagaData : ISagaData
{
    public Guid Id { get; set; }
    public int Revision { get; set; }

    // UI fields
    public string? ArtistName { get; set; }
    public string? ReleaseName { get; set; }
    public string? ReleaseYear { get; set; }
    public string StatusDescription { get; set; }
    public int? NumberOfTracks { get; set; } = null;
    public int? TracksDownloaded { get; set; } = null;

    // Data
    public string MusicBrainzReleaseId { get; set; }
    public Release? Release { get; set; }
    public List<Recording>? Recordings { get; set; }
}
