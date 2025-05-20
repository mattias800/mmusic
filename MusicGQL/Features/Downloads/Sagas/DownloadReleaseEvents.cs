namespace MusicGQL.Features.Downloads.Sagas;

// Events
public record DownloadReleaseQueuedEvent(string MusicBrainzReleaseId);

// Actions
public record LookupReleaseInMusicBrainz(string MusicBrainzReleaseId);

public record LookupRecordingsForReleaseInMusicBrainz(
    string MusicBrainzReleaseId,
    Hqub.MusicBrainz.Entities.Release Release
);

public record FoundReleaseInMusicBrainz(
    string MusicBrainzReleaseId,
    Hqub.MusicBrainz.Entities.Release Release
);

public record FoundRecordingsForReleaseInMusicBrainz(
    string MusicBrainzReleaseId,
    Hqub.MusicBrainz.Entities.Release Release,
    List<Hqub.MusicBrainz.Entities.Recording> Recordings
);

public record ReleaseNotFoundInMusicBrainz(string MusicBrainzReleaseId);

public record NoRecordingsFoundInMusicBrainz(string MusicBrainzReleaseId);
