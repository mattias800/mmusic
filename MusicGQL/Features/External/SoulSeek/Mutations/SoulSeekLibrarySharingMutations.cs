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
}

#region Start Sharing

[UnionType]
public abstract record StartSoulSeekSharingResult;

public record StartSoulSeekSharingSuccess : StartSoulSeekSharingResult;

public record StartSoulSeekSharingError(string Message) : StartSoulSeekSharingResult;

#endregion

#region Stop Sharing

[UnionType]
public abstract record StopSoulSeekSharingResult;

public record StopSoulSeekSharingSuccess : StopSoulSeekSharingResult;

public record StopSoulSeekSharingError(string Message) : StopSoulSeekSharingResult;

#endregion

#region Refresh Shares

[UnionType]
public abstract record RefreshSoulSeekSharesResult;

public record RefreshSoulSeekSharesSuccess : RefreshSoulSeekSharesResult;

public record RefreshSoulSeekSharesError(string Message) : RefreshSoulSeekSharesResult;

#endregion
