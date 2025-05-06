using Hqub.MusicBrainz.Entities;

namespace MusicGQL.Sagas.DownloadRelease;

// Events
public record DownloadReleaseQueuedEvent(string MusicBrainzReleaseId);

// Actions
public record LookupReleaseInMusicBrainz(string MusicBrainzReleaseId);

public record FoundReleaseInMusicBrainz(Release Release);

public record ReleaseNotFoundInMusicBrainz(string MusicBrainzReleaseId);

public record SearchReleaseDownload(Release Release);

public record FoundReleaseDownload(Release Release);