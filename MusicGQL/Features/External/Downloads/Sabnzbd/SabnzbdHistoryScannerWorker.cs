using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Writer;
using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.External.Downloads.Sabnzbd;

public class SabnzbdHistoryScannerOptions
{
    public bool Enabled { get; set; } = true;
    public int ScanIntervalMinutes { get; set; } = 5; // Check every 5 minutes
    public int MaxHistoryItemsToProcess { get; set; } = 50; // Process up to 50 history items per scan
    public int MaxAgeHours { get; set; } = 24; // Only process items from last 24 hours
}

public class SabnzbdHistoryScannerWorker(
    IOptions<SabnzbdOptions> sabnzbdOptions,
    IOptions<SabnzbdHistoryScannerOptions> options,
    ILogger<SabnzbdHistoryScannerWorker> logger,
    ServerLibraryCache cache,
    ServerSettingsAccessor serverSettingsAccessor,
    IServiceScopeFactory scopeFactory,
    HttpClient httpClient
) : BackgroundService
{
    private readonly ConcurrentDictionary<string, DateTime> _processedJobs = new();
    private readonly ConcurrentDictionary<string, DateTime> _failedMatches = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = options?.Value ?? new SabnzbdHistoryScannerOptions();
        if (!opts.Enabled)
        {
            logger.LogInformation("[SAB History Scanner] Service disabled");
            return;
        }

        logger.LogInformation(
            "[SAB History Scanner] Started with {Interval} minute scan interval",
            opts.ScanIntervalMinutes
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanHistoryForMissingReleasesAsync(opts, stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(opts.ScanIntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[SAB History Scanner] Error during history scan");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait 1 minute on error
            }
        }
    }

    private async Task ScanHistoryForMissingReleasesAsync(
        SabnzbdHistoryScannerOptions opts,
        CancellationToken cancellationToken
    )
    {
        var baseUrl = sabnzbdOptions.Value.BaseUrl?.TrimEnd('/');
        var apiKey = sabnzbdOptions.Value.ApiKey;

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogDebug("[SAB History Scanner] Missing SABnzbd configuration, skipping scan");
            return;
        }

        try
        {
            // Get SABnzbd history
            var historyUrl =
                $"{baseUrl}/api?mode=history&apikey={Uri.EscapeDataString(apiKey)}&output=json&limit={opts.MaxHistoryItemsToProcess}";
            logger.LogDebug("[SAB History Scanner] Querying SABnzbd history at {Url}", historyUrl);

            var response = await httpClient.GetAsync(historyUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "[SAB History Scanner] Failed to get history: HTTP {Status}",
                    (int)response.StatusCode
                );
                return;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var doc = JsonDocument.Parse(json);

            if (
                !doc.RootElement.TryGetProperty("history", out var history)
                || !history.TryGetProperty("slots", out var slots)
            )
            {
                logger.LogDebug("[SAB History Scanner] No history slots found in response");
                return;
            }

            var cutoffTime = DateTime.UtcNow.AddHours(-opts.MaxAgeHours);
            var processedCount = 0;
            var matchedCount = 0;

            foreach (var slot in slots.EnumerateArray())
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                if (processedCount >= opts.MaxHistoryItemsToProcess)
                    break;

                try
                {
                    // Log the slot structure for debugging
                    logger.LogDebug(
                        "[SAB History Scanner] Processing slot: {SlotJson}",
                        slot.GetRawText()
                    );

                    if (
                        slot.TryGetProperty("name", out var name)
                        && slot.TryGetProperty("completed", out var completed)
                        && slot.TryGetProperty("status", out var status)
                    )
                    {
                        var jobName = name.GetString();
                        var statusStr = status.GetString();

                        logger.LogDebug(
                            "[SAB History Scanner] Slot has name='{JobName}', status='{Status}', completed type={CompletedType}",
                            jobName,
                            statusStr,
                            completed.ValueKind
                        );

                        if (string.IsNullOrWhiteSpace(jobName) || statusStr != "Completed")
                        {
                            continue;
                        }

                        // Handle completed field - it can be either a string or a number (timestamp)
                        DateTime completedTime;
                        if (completed.ValueKind == JsonValueKind.String)
                        {
                            var completedStr = completed.GetString();
                            if (
                                string.IsNullOrWhiteSpace(completedStr)
                                || !DateTime.TryParse(completedStr, out completedTime)
                            )
                            {
                                logger.LogDebug(
                                    "[SAB History Scanner] Failed to parse completed string: '{CompletedStr}'",
                                    completedStr
                                );
                                continue;
                            }
                        }
                        else if (completed.ValueKind == JsonValueKind.Number)
                        {
                            // SABnzbd sometimes returns Unix timestamps
                            var timestamp = completed.GetInt64();
                            completedTime = DateTimeOffset
                                .FromUnixTimeSeconds(timestamp)
                                .UtcDateTime;
                            logger.LogDebug(
                                "[SAB History Scanner] Parsed Unix timestamp {Timestamp} -> {CompletedTime}",
                                timestamp,
                                completedTime
                            );
                        }
                        else
                        {
                            // Skip if we can't parse the completion time
                            logger.LogDebug(
                                "[SAB History Scanner] Skipping slot with unexpected completed type: {CompletedType}",
                                completed.ValueKind
                            );
                            continue;
                        }

                        if (completedTime < cutoffTime)
                        {
                            logger.LogDebug(
                                "[SAB History Scanner] Slot completed at {CompletedTime} is older than cutoff {CutoffTime}",
                                completedTime,
                                cutoffTime
                            );
                            continue;
                        }

                        // Skip if we've already processed this job
                        if (
                            _processedJobs.TryGetValue(jobName, out var lastProcessed)
                            && lastProcessed > completedTime
                        )
                        {
                            continue;
                        }

                        processedCount++;
                        logger.LogDebug(
                            "[SAB History Scanner] Processing completed job: {JobName} (completed: {Completed})",
                            jobName,
                            completedTime
                        );

                        // Try to match this job against missing releases
                        var matched = await TryMatchJobToMissingReleaseAsync(
                            jobName,
                            cancellationToken
                        );
                        if (matched)
                        {
                            matchedCount++;
                            _processedJobs.AddOrUpdate(
                                jobName,
                                completedTime,
                                (_, _) => completedTime
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[SAB History Scanner] Error processing history slot");
                }
            }

            if (processedCount > 0)
            {
                logger.LogInformation(
                    "[SAB History Scanner] Processed {Processed} history items, matched {Matched} to missing releases",
                    processedCount,
                    matchedCount
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SAB History Scanner] Error scanning SABnzbd history");
        }
    }

    private async Task<bool> TryMatchJobToMissingReleaseAsync(
        string jobName,
        CancellationToken cancellationToken
    )
    {
        try
        {
            // Skip if we recently failed to match this job
            if (
                _failedMatches.TryGetValue(jobName, out var lastFailed)
                && lastFailed > DateTime.UtcNow.AddHours(-1)
            ) // 1 hour cooldown on failed matches
            {
                return false;
            }

            // Get all missing releases (releases with no available tracks)
            var allReleases = await cache.GetAllReleasesAsync();
            var missingReleases = allReleases
                .Where(r =>
                    (r.Tracks?.Count ?? 0) > 0
                    && r.Tracks.All(t =>
                        t.CachedMediaAvailabilityStatus != CachedMediaAvailabilityStatus.Available
                    )
                )
                .ToList();

            if (missingReleases.Count == 0)
            {
                logger.LogDebug("[SAB History Scanner] No missing releases found in library");
                return false;
            }

            // Try to find a match by comparing job name with artist/album names
            var bestMatch = FindBestReleaseMatch(jobName, missingReleases);
            if (bestMatch == null)
            {
                logger.LogDebug(
                    "[SAB History Scanner] No match found for job '{JobName}'",
                    jobName
                );
                _failedMatches.AddOrUpdate(jobName, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
                return false;
            }

            logger.LogInformation(
                "[SAB History Scanner] Found match for job '{JobName}' -> {ArtistId}/{ReleaseFolder}",
                jobName,
                bestMatch.ArtistId,
                bestMatch.FolderName
            );

            // Try to finalize the release
            var settings = await serverSettingsAccessor.GetAsync();
            var completedPath = sabnzbdOptions.Value.CompletedPath;

            if (string.IsNullOrWhiteSpace(completedPath))
            {
                logger.LogWarning("[SAB History Scanner] No completed path configured for SABnzbd");
                return false;
            }

            // Look for the release in the completed downloads folder
            var potentialPaths = new List<string>
            {
                System.IO.Path.Combine(
                    completedPath,
                    "mmusic",
                    bestMatch.ArtistId,
                    bestMatch.FolderName
                ),
                System.IO.Path.Combine(completedPath, bestMatch.ArtistId, bestMatch.FolderName),
                System.IO.Path.Combine(completedPath, bestMatch.FolderName),
            };

            foreach (var potentialPath in potentialPaths)
            {
                if (Directory.Exists(potentialPath))
                {
                    logger.LogInformation(
                        "[SAB History Scanner] Found potential release at {Path}, attempting finalization",
                        potentialPath
                    );

                    using var scope = scopeFactory.CreateScope();
                    var finalizeService =
                        scope.ServiceProvider.GetRequiredService<SabnzbdFinalizeService>();

                    try
                    {
                        await finalizeService.FinalizeReleaseAsync(
                            bestMatch.ArtistId,
                            bestMatch.FolderName,
                            cancellationToken
                        );
                        _processedJobs.AddOrUpdate(
                            jobName,
                            DateTime.UtcNow,
                            (_, _) => DateTime.UtcNow
                        );
                        return true;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(
                            ex,
                            "[SAB History Scanner] Failed to finalize release at {Path}",
                            potentialPath
                        );
                    }
                }
            }

            logger.LogDebug(
                "[SAB History Scanner] No matching directory found for job '{JobName}'",
                jobName
            );
            _failedMatches.AddOrUpdate(jobName, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[SAB History Scanner] Error trying to match job '{JobName}'",
                jobName
            );
            return false;
        }
    }

    private static CachedRelease? FindBestReleaseMatch(
        string jobName,
        List<CachedRelease> missingReleases
    )
    {
        // Simple matching logic - can be improved later
        var jobNameLower = jobName.ToLowerInvariant();

        // First try exact matches
        var exactMatch = missingReleases.FirstOrDefault(r =>
            jobNameLower.Contains(r.ArtistId.ToLowerInvariant())
            && jobNameLower.Contains(r.FolderName.ToLowerInvariant())
        );

        if (exactMatch != null)
            return exactMatch;

        // Try partial matches
        var partialMatch = missingReleases.FirstOrDefault(r =>
            jobNameLower.Contains(r.ArtistId.ToLowerInvariant())
            || jobNameLower.Contains(r.FolderName.ToLowerInvariant())
        );

        return partialMatch;
    }
}
