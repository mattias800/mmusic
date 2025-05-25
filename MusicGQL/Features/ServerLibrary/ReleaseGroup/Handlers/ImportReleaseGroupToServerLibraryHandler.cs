using HotChocolate.Subscriptions;
using MusicGQL.Features.ServerLibrary.ArtistServerStatus;
using MusicGQL.Features.ServerLibrary.ArtistServerStatus.Services;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;

public class ImportReleaseGroupToServerLibraryHandler(
    IDriver driver,
    ITopicEventSender sender,
    MusicBrainzService mbService,
    ServerLibraryService serverLibraryService,
    ArtistServerStatusService artistServerStatusService,
    ILogger<ImportReleaseGroupToServerLibraryHandler> logger
)
{
    public async Task Handle(Command command)
    {
        try
        {
            logger.LogInformation(
                "Importing release group, id={ReleaseGroupMbId}",
                command.ReleaseGroupMbId
            );

            var releaseGroup = await mbService.GetReleaseGroupByIdAsync(command.ReleaseGroupMbId);

            if (releaseGroup is null)
            {
                logger.LogError(
                    "Release group {MessageReleaseGroupMbId} not found in MusicBrainz",
                    command.ReleaseGroupMbId
                );
                return;
            }

            logger.LogInformation(
                "Found release group, id={ReleaseGroupMbId}: {ReleaseGroupTitle}",
                command.ReleaseGroupMbId,
                releaseGroup.Title
            );

            var artistId = releaseGroup.Credits?.First().Artist.Id;

            await PublishStartEvent(artistId);

            logger.LogInformation(
                "Fetching all releases for release group {Title}",
                releaseGroup.Title
            );

            var allReleases = await mbService.GetReleasesForReleaseGroupAsync(releaseGroup.Id);

            logger.LogInformation(
                "Fetched {ReleaseCount} releases for release group {Title}",
                allReleases.Count,
                releaseGroup.Title
            );

            var mainRelease = LibraryDecider.GetMainReleaseInReleaseGroup(allReleases.ToList());

            if (mainRelease == null)
            {
                logger.LogWarning(
                    "No main release found for release group {Title} after fetching {Count} releases. Only the release group itself will be persisted",
                    releaseGroup.Title,
                    allReleases.Count
                );
                await PublishEndEvent(artistId);
                return;
            }

            logger.LogInformation(
                "Prioritized main release for release group {RgTitle} is {MainReleaseTitle} ({MainReleaseId})",
                releaseGroup.Title,
                mainRelease.Title,
                mainRelease.Id
            );

            await SaveReleaseGroupInDatabase(releaseGroup, mainRelease);
            await PublishEndEvent(artistId);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error while importing release group {ReleaseGroupMbId}",
                command.ReleaseGroupMbId
            );
        }
    }

    private async Task PublishStartEvent(string? artistId)
    {
        if (artistId == null)
        {
            return;
        }

        if (
            artistServerStatusService.GetStatus(artistId).Status
            != ArtistServerStatusWorkingStatus.ImportingArtistReleases
        )
        {
            artistServerStatusService.SetImportingArtistReleasesStatus(artistId, 0, 0);
        }

        artistServerStatusService.IncreaseTotalNumReleaseGroupsBeingImported(artistId);

        await sender.SendAsync(
            ArtistServerStatusSubscription.ArtistServerStatusUpdatedTopic(artistId),
            new ArtistServerStatus.ArtistServerStatus(artistId)
        );
    }

    private async Task PublishEndEvent(string? artistId)
    {
        if (artistId == null)
        {
            return;
        }

        artistServerStatusService.IncreaseNumReleaseGroupsFinishedImporting(artistId);
        artistServerStatusService.SetReadyStatusIfImportDone(artistId);

        await sender.SendAsync(
            ArtistServerStatusSubscription.ArtistServerStatusUpdatedTopic(artistId),
            new ArtistServerStatus.ArtistServerStatus(artistId)
        );
    }

    private async Task SaveReleaseGroupInDatabase(
        Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup,
        Hqub.MusicBrainz.Entities.Release mainRelease
    )
    {
        await using var session = driver.AsyncSession();
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await serverLibraryService.SaveReleaseGroupNodeAsync(
                    (IAsyncTransaction)tx,
                    releaseGroup
                );
                logger.LogInformation("Saved release group {Title}", releaseGroup.Title);

                // 2. Save ReleaseGroup Artist Credits
                if (releaseGroup.Credits != null)
                {
                    await serverLibraryService.SaveArtistCreditsForParentAsync(
                        (IAsyncTransaction)tx,
                        releaseGroup.Id,
                        releaseGroup.Credits,
                        "ReleaseGroup",
                        "rgId",
                        "CREDITED_ON_RELEASE_GROUP"
                    );
                    logger.LogInformation(
                        "Saved artist credits for release group {Title}",
                        releaseGroup.Title
                    );
                }

                // 3. Process ONLY the Main Release (if found)
                if (mainRelease != null)
                {
                    await serverLibraryService.SaveReleaseNodeAsync(
                        (IAsyncTransaction)tx,
                        mainRelease
                    );
                    await serverLibraryService.LinkReleaseToReleaseGroupAsync(
                        (IAsyncTransaction)tx,
                        releaseGroup.Id,
                        mainRelease.Id
                    );
                    logger.LogInformation(
                        "Saved Main Release {ReleaseTitle} ({ReleaseId}) and linked to RG {RgTitle}",
                        mainRelease.Title,
                        mainRelease.Id,
                        releaseGroup.Title
                    );

                    // 4. Save Main Release Artist Credits
                    if (mainRelease.Credits != null)
                    {
                        await serverLibraryService.SaveArtistCreditsForParentAsync(
                            (IAsyncTransaction)tx,
                            mainRelease.Id,
                            mainRelease.Credits,
                            "Release",
                            "releaseId",
                            "CREDITED_ON_RELEASE"
                        );
                        logger.LogInformation(
                            "Saved artist credits for main release {Title}",
                            mainRelease.Title
                        );
                    }

                    // 5. Process Media and Tracks for the Main Release
                    if (mainRelease.Media != null)
                    {
                        foreach (var mediumDto in mainRelease.Media)
                        {
                            if (mediumDto == null)
                                continue;

                            string mediumNodeId = $"{mainRelease.Id}_m{mediumDto.Position}";
                            await serverLibraryService.SaveMediumNodeAsync(
                                (IAsyncTransaction)tx,
                                mediumNodeId,
                                mainRelease.Id,
                                mediumDto
                            );
                            logger.LogInformation(
                                "Saved Medium {MediumId} (Pos: {MediumPos}) for Main Release {ReleaseTitle}",
                                mediumNodeId,
                                mediumDto.Position,
                                mainRelease.Title
                            );

                            if (mediumDto.Tracks != null)
                            {
                                foreach (var trackDto in mediumDto.Tracks)
                                {
                                    if (
                                        trackDto?.Recording == null
                                        || string.IsNullOrEmpty(trackDto.Recording.Id)
                                    )
                                        continue;

                                    await serverLibraryService.SaveRecordingNodeAsync(
                                        (IAsyncTransaction)tx,
                                        trackDto.Recording
                                    );

                                    await serverLibraryService.LinkTrackOnMediumToRecordingAsync(
                                        (IAsyncTransaction)tx,
                                        mediumNodeId,
                                        trackDto.Recording.Id,
                                        trackDto
                                    );
                                    logger.LogInformation(
                                        "Linked Track (Pos:{TrackPos}, Num:{TrackNum}, Title:'{TrackTitle}') on Medium {MediumId} to Recording {RecordingTitle} ({RecordingId})",
                                        trackDto.Position,
                                        trackDto.Number,
                                        trackDto.Recording?.Title ?? string.Empty,
                                        mediumNodeId,
                                        trackDto.Recording?.Title ?? string.Empty,
                                        trackDto.Recording?.Id ?? string.Empty
                                    );

                                    // 6. Save Recording Artist Credits
                                    if (trackDto.Recording?.Credits != null)
                                    {
                                        await serverLibraryService.SaveArtistCreditsForParentAsync(
                                            (IAsyncTransaction)tx,
                                            trackDto.Recording.Id,
                                            trackDto.Recording.Credits,
                                            "Recording",
                                            "recordingId",
                                            "CREDITED_ON_RECORDING"
                                        );
                                        logger.LogInformation(
                                            "Saved artist credits for recording {RecordingTitle}",
                                            trackDto.Recording?.Title ?? string.Empty
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            });

            logger.LogInformation(
                "Successfully persisted data for release group {Title}",
                releaseGroup.Title
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error persisting data for release group {Title}: {ExMessage}",
                releaseGroup.Title,
                ex.Message
            );
        }
    }

    public record Command(string ReleaseGroupMbId);
}
