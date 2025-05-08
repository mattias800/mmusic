using Hqub.MusicBrainz.Entities;

namespace MusicGQL.Sagas.DownloadRelease;

// Events
public record DownloadReleaseQueuedEvent(string MusicBrainzReleaseId);

// Actions
public record LookupReleaseInMusicBrainz(string MusicBrainzReleaseId);

public record LookupRecordingsForReleaseInMusicBrainz(string MusicBrainzReleaseId, Release Release);

public record FoundReleaseInMusicBrainz(string MusicBrainzReleaseId, Release Release);

public record FoundRecordingsForReleaseInMusicBrainz(
    string MusicBrainzReleaseId,
    Release Release,
    List<Recording> Recordings
);

public record ReleaseNotFoundInMusicBrainz(string MusicBrainzReleaseId);

public record NoRecordingsFoundInMusicBrainz(string MusicBrainzReleaseId);
