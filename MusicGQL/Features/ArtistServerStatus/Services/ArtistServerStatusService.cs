using System.Collections.Concurrent;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.ArtistServerStatus.Services;

public class ArtistServerStatusService(ITopicEventSender sender, IServiceScopeFactory scopeFactory)
{
    private readonly ConcurrentDictionary<string, ArtistServerStatusWorkingState> _artistStatus =
        new();

    public ArtistServerStatusWorkingState GetStatus(string artistId)
    {
        var s = _artistStatus.GetValueOrDefault(artistId);
        if (s is not null)
        {
            return s;
        }

        _artistStatus[artistId] = new ArtistServerStatusWorkingState
        {
            Status = ArtistServerStatusWorkingStatus.ImportingArtistReleases,
            TotalNumReleaseGroupsBeingImported = 1,
            NumReleaseGroupsFinishedImporting = 0,
        };

        return _artistStatus[artistId];
    }

    public void SetStatus(string artistId, ArtistServerStatusWorkingState status)
    {
        _artistStatus[artistId] = status;
        Publish(artistId);
    }

    public void SetImportingArtistStatus(string artistId)
    {
        SetStatus(
            artistId,
            new ArtistServerStatusWorkingState
            {
                Status = ArtistServerStatusWorkingStatus.ImportingArtist,
                TotalNumReleaseGroupsBeingImported = 0,
                NumReleaseGroupsFinishedImporting = 0,
            }
        );
    }

    public void SetImportingArtistReleasesStatus(
        string artistId,
        int numReleaseGroupsFinishedImporting,
        int totalNumReleaseGroupsBeingImported
    )
    {
        SetStatus(
            artistId,
            new ArtistServerStatusWorkingState
            {
                Status = ArtistServerStatusWorkingStatus.ImportingArtistReleases,
                TotalNumReleaseGroupsBeingImported = totalNumReleaseGroupsBeingImported,
                NumReleaseGroupsFinishedImporting = numReleaseGroupsFinishedImporting,
            }
        );
    }

    public void SetReadyStatus(string artistId)
    {
        SetStatus(
            artistId,
            new ArtistServerStatusWorkingState
            {
                Status = ArtistServerStatusWorkingStatus.Ready,
                TotalNumReleaseGroupsBeingImported = 0,
                NumReleaseGroupsFinishedImporting = 0,
            }
        );
    }

    public void SetReadyStatusIfImportDone(string artistId)
    {
        var s = _artistStatus.GetValueOrDefault(artistId);
        if (s is not null)
        {
            if (s.TotalNumReleaseGroupsBeingImported == s.NumReleaseGroupsFinishedImporting)
            {
                SetStatus(
                    artistId,
                    new ArtistServerStatusWorkingState
                    {
                        Status = ArtistServerStatusWorkingStatus.Ready,
                        TotalNumReleaseGroupsBeingImported = 0,
                        NumReleaseGroupsFinishedImporting = 0,
                    }
                );
            }
        }
    }

    public void IncreaseTotalNumReleaseGroupsBeingImported(string artistId)
    {
        if (_artistStatus.TryGetValue(artistId, out var state))
        {
            state.TotalNumReleaseGroupsBeingImported++;
            Publish(artistId);
        }
        else
        {
            SetStatus(
                artistId,
                new ArtistServerStatusWorkingState
                {
                    Status = ArtistServerStatusWorkingStatus.ImportingArtistReleases,
                    TotalNumReleaseGroupsBeingImported = 1,
                    NumReleaseGroupsFinishedImporting = 0,
                }
            );
        }
    }

    public void IncreaseNumReleaseGroupsFinishedImporting(string artistId)
    {
        if (_artistStatus.TryGetValue(artistId, out var state))
        {
            state.NumReleaseGroupsFinishedImporting++;
            Publish(artistId);
        }
        else
        {
            SetStatus(
                artistId,
                new ArtistServerStatusWorkingState
                {
                    Status = ArtistServerStatusWorkingStatus.ImportingArtistReleases,
                    TotalNumReleaseGroupsBeingImported = 0,
                    NumReleaseGroupsFinishedImporting = 1,
                }
            );
        }
    }

    public async Task<ArtistServerStatusResult> GetArtistServerStatus(string artistId)
    {
        var state = _artistStatus.GetValueOrDefault(artistId);
        return state is null ? await CreateForNoState(artistId) : CreateForState(state, artistId);
    }

    private ArtistServerStatusResult CreateForState(
        ArtistServerStatusWorkingState state,
        string artistId
    )
    {
        return state.Status switch
        {
            ArtistServerStatusWorkingStatus.ImportingArtist =>
                new ArtistServerStatusImportingArtist(artistId),
            ArtistServerStatusWorkingStatus.ImportingArtistReleases =>
                new ArtistServerStatusImportingArtistReleases(
                    artistId,
                    state.NumReleaseGroupsFinishedImporting,
                    state.TotalNumReleaseGroupsBeingImported
                ),
            ArtistServerStatusWorkingStatus.NotInLibrary => new ArtistServerStatusNotInLibrary(
                artistId
            ),
            ArtistServerStatusWorkingStatus.Ready => new ArtistServerStatusReady(artistId),
            ArtistServerStatusWorkingStatus.UpdatingArtist => new ArtistServerStatusUpdatingArtist(
                artistId
            ),
            _ => new ArtistServerStatusReady(artistId),
        };
    }

    private async Task<ArtistServerStatusResult> CreateForNoState(string artistId)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();

        // Check if the artist is in any user's server library
        var isInLibrary = await dbContext.ServerArtists.AnyAsync(sa => sa.ArtistId == artistId);
        return isInLibrary
            ? new ArtistServerStatusReady(artistId)
            : new ArtistServerStatusNotInLibrary(artistId);
    }

    private void Publish(string artistId)
    {
        _ = sender.SendAsync(
            ArtistServerStatusSubscription.ArtistServerStatusUpdatedTopic(artistId),
            new ArtistServerStatus(artistId)
        );
    }
}

public enum ArtistServerStatusWorkingStatus
{
    NotInLibrary,
    ImportingArtist,
    UpdatingArtist,
    ImportingArtistReleases,
    Ready,
}

public class ArtistServerStatusWorkingState
{
    public ArtistServerStatusWorkingStatus Status { get; set; }
    public int NumReleaseGroupsFinishedImporting { get; set; }
    public int TotalNumReleaseGroupsBeingImported { get; set; }
}
