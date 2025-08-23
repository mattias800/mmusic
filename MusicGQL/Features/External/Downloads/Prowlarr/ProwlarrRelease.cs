namespace MusicGQL.Features.External.Downloads.Prowlarr;

public record ProwlarrRelease(
    string? Title,
    string? Guid,
    string? MagnetUrl,
    string? DownloadUrl,
    long? Size,
    int? IndexerId);

