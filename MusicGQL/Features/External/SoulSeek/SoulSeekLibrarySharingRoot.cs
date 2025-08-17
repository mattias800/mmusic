using HotChocolate;
using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Types;

namespace MusicGQL.Features.External.SoulSeek;

/// <summary>
/// GraphQL query root for Soulseek library sharing
/// </summary>
[ExtendObjectType(typeof(Query))]
public class SoulSeekLibrarySharingRoot
{
    /// <summary>
    /// Gets the current Soulseek library sharing statistics
    /// </summary>
    public async Task<SharingStatistics> GetSoulSeekSharingStatistics(
        [Service] SoulSeekLibrarySharingService sharingService
    )
    {
        return await sharingService.GetSharingStatisticsAsync();
    }
}
