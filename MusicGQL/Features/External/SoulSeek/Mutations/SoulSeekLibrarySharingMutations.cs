using HotChocolate;
using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Types;

namespace MusicGQL.Features.External.SoulSeek.Mutations;

/// <summary>
/// GraphQL mutations for controlling Soulseek library sharing
/// </summary>
[ExtendObjectType(typeof(Mutation))]
public class SoulSeekLibrarySharingMutations
{
    /// <summary>
    /// Starts sharing the music library on Soulseek
    /// </summary>
    public async Task<StartSoulSeekSharingResult> StartSoulSeekSharing(
        [Service] SoulSeekLibrarySharingService sharingService
    )
    {
        try
        {
            await sharingService.StartSharingAsync();
            return new StartSoulSeekSharingSuccess();
        }
        catch (Exception ex)
        {
            return new StartSoulSeekSharingError(ex.Message);
        }
    }

    /// <summary>
    /// Stops sharing the music library on Soulseek
    /// </summary>
    public async Task<StopSoulSeekSharingResult> StopSoulSeekSharing(
        [Service] SoulSeekLibrarySharingService sharingService
    )
    {
        try
        {
            await sharingService.StopSharingAsync();
            return new StopSoulSeekSharingSuccess();
        }
        catch (Exception ex)
        {
            return new StopSoulSeekSharingError(ex.Message);
        }
    }

    /// <summary>
    /// Refreshes the Soulseek share index
    /// </summary>
    public async Task<RefreshSoulSeekSharesResult> RefreshSoulSeekShares(
        [Service] SoulSeekLibrarySharingService sharingService
    )
    {
        try
        {
            await sharingService.RefreshSharesAsync();
            return new RefreshSoulSeekSharesSuccess();
        }
        catch (Exception ex)
        {
            return new RefreshSoulSeekSharesError(ex.Message);
        }
    }

    /// <summary>
    /// Triggers a reachability check and returns the latest sharing statistics (including reachability info)
    /// </summary>
    public async Task<CheckSoulSeekReachabilityResult> CheckSoulSeekReachability(
        [Service] SoulSeekLibrarySharingService sharingService
    )
    {
        try
        {
            await sharingService.CheckReachabilityAsync();
            var stats = await sharingService.GetSharingStatisticsAsync();
            return new CheckSoulSeekReachabilitySuccess(stats);
        }
        catch (Exception ex)
        {
            return new CheckSoulSeekReachabilityError(ex.Message);
        }
    }
}

#region Start Sharing

[UnionType("StartSoulSeekSharingResult")]
public abstract record StartSoulSeekSharingResult;

public record StartSoulSeekSharingSuccess(bool Ok = true) : StartSoulSeekSharingResult;

public record StartSoulSeekSharingError(string Message) : StartSoulSeekSharingResult;

#endregion

#region Stop Sharing

[UnionType("StopSoulSeekSharingResult")]
public abstract record StopSoulSeekSharingResult;

public record StopSoulSeekSharingSuccess(bool Ok = true) : StopSoulSeekSharingResult;

public record StopSoulSeekSharingError(string Message) : StopSoulSeekSharingResult;

#endregion

#region Refresh Shares

[UnionType("RefreshSoulSeekSharesResult")]
public abstract record RefreshSoulSeekSharesResult;

public record RefreshSoulSeekSharesSuccess(bool Ok = true) : RefreshSoulSeekSharesResult;

public record RefreshSoulSeekSharesError(string Message) : RefreshSoulSeekSharesResult;

#endregion

#region Check Reachability

[UnionType("CheckSoulSeekReachabilityResult")]
public abstract record CheckSoulSeekReachabilityResult;

public record CheckSoulSeekReachabilitySuccess(SharingStatistics Statistics)
    : CheckSoulSeekReachabilityResult;

public record CheckSoulSeekReachabilityError(string Message) : CheckSoulSeekReachabilityResult;

#endregion
