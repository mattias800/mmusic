using System.Collections.Concurrent;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.ServerLibrary.ArtistServerStatus.Services;

public class ArtistServerStatusService(EventDbContext dbContext)
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
    }

    public void SetImportingArtistStatus(string artistId)
    {
        _artistStatus[artistId] = new ArtistServerStatusWorkingState
        {
            Status = ArtistServerStatusWorkingStatus.ImportingArtist,
            TotalNumReleaseGroupsBeingImported = 0,
            NumReleaseGroupsFinishedImporting = 0,
        };
    }

    public void SetImportingArtistReleasesStatus(
        string artistId,
        int numReleaseGroupsFinishedImporting,
        int totalNumReleaseGroupsBeingImported
    )
    {
        _artistStatus[artistId] = new ArtistServerStatusWorkingState
        {
            Status = ArtistServerStatusWorkingStatus.ImportingArtistReleases,
            TotalNumReleaseGroupsBeingImported = totalNumReleaseGroupsBeingImported,
            NumReleaseGroupsFinishedImporting = numReleaseGroupsFinishedImporting,
        };
    }

    public void SetReadyStatus(string artistId)
    {
        _artistStatus[artistId] = new ArtistServerStatusWorkingState
        {
            Status = ArtistServerStatusWorkingStatus.Ready,
            TotalNumReleaseGroupsBeingImported = 0,
            NumReleaseGroupsFinishedImporting = 0,
        };
    }

    public void SetReadyStatusIfImportDone(string artistId)
    {
        var s = _artistStatus.GetValueOrDefault(artistId);
        if (s is not null)
        {
            if (s.TotalNumReleaseGroupsBeingImported == s.NumReleaseGroupsFinishedImporting)
            {
                _artistStatus[artistId] = new ArtistServerStatusWorkingState
                {
                    Status = ArtistServerStatusWorkingStatus.Ready,
                    TotalNumReleaseGroupsBeingImported = 0,
                    NumReleaseGroupsFinishedImporting = 0,
                };
            }
        }
    }

    public void IncreaseTotalNumReleaseGroupsBeingImported(string artistId)
    {
        if (_artistStatus.TryGetValue(artistId, out var state))
        {
            state.TotalNumReleaseGroupsBeingImported++;
        }
        else
        {
            _artistStatus[artistId] = new ArtistServerStatusWorkingState
            {
                Status = ArtistServerStatusWorkingStatus.ImportingArtistReleases,
                TotalNumReleaseGroupsBeingImported = 1,
                NumReleaseGroupsFinishedImporting = 0,
            };
        }
    }

    public void IncreaseNumReleaseGroupsFinishedImporting(string artistId)
    {
        if (_artistStatus.TryGetValue(artistId, out var state))
        {
            state.NumReleaseGroupsFinishedImporting++;
        }
        else
        {
            _artistStatus[artistId] = new ArtistServerStatusWorkingState
            {
                Status = ArtistServerStatusWorkingStatus.ImportingArtistReleases,
                TotalNumReleaseGroupsBeingImported = 0,
                NumReleaseGroupsFinishedImporting = 1,
            };
        }
    }

    public async Task<ArtistServerStatusResult> GetArtistServerStatus(string artistId)
    {
        var state = _artistStatus.GetValueOrDefault(artistId);
        return state is null ? await CreateForNoState(artistId) : CreateForState(state);
    }

    private ArtistServerStatusResult CreateForState(ArtistServerStatusWorkingState state)
    {
        return state.Status switch
        {
            ArtistServerStatusWorkingStatus.ImportingArtist =>
                new ArtistServerStatusImportingArtist(),
            ArtistServerStatusWorkingStatus.ImportingArtistReleases =>
                new ArtistServerStatusImportingArtistReleases(
                    state.NumReleaseGroupsFinishedImporting,
                    state.TotalNumReleaseGroupsBeingImported
                ),
            ArtistServerStatusWorkingStatus.NotInLibrary => new ArtistServerStatusNotInLibrary(),
            ArtistServerStatusWorkingStatus.Ready => new ArtistServerStatusReady(),
            ArtistServerStatusWorkingStatus.UpdatingArtist =>
                new ArtistServerStatusUpdatingArtist(),
            _ => new ArtistServerStatusReady(),
        };
    }

    private async Task<ArtistServerStatusResult> CreateForNoState(string artistId)
    {
        var artist = await dbContext.ArtistsAddedToServerLibraryProjection.FindAsync(1);
        var isInLibrary = artist?.ArtistMbIds.Contains(artistId) ?? false;
        return isInLibrary ? new ArtistServerStatusReady() : new ArtistServerStatusNotInLibrary();
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
