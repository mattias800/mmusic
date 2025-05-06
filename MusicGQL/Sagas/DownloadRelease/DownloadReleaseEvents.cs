using Hqub.MusicBrainz.Entities;

namespace MusicGQL.Sagas.DownloadRelease;

// Events
public record DownloadReleaseQueuedEvent(string MusicBrainzReleaseId);

// Actions
public record LookupReleaseInMusicBrainz(string MusicBrainzReleaseId);

public record FoundReleaseInMusicBrainz(string MusicBrainzReleaseId, Release Release);

public record ReleaseNotFoundInMusicBrainz(string MusicBrainzReleaseId);

public record SearchReleaseDownload(string MusicBrainzReleaseId, Release Release);

public record FoundReleaseDownload(string MusicBrainzReleaseId, Release Release);