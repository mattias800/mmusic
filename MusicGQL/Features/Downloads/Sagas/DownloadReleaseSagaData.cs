using Rebus.Sagas;

namespace MusicGQL.Features.Downloads.Sagas;

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
    public Hqub.MusicBrainz.Entities.Release? Release { get; set; }
    public List<Hqub.MusicBrainz.Entities.Recording>? Recordings { get; set; }

    public Queue<DownloadQueueItem>? DownloadQueue { get; set; }
}

public record DownloadQueueItem(string Username, string FileName, string LocalFileName);
