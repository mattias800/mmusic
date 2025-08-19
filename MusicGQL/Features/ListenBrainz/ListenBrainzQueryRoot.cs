using HotChocolate;
using MusicGQL.Integration.ListenBrainz;
using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.ListenBrainz;

public record ListenBrainzQueryRoot
{
    public async Task<ListenBrainzUserInfo?> GetUserInfo(
        [Service] ListenBrainzService listenBrainzService,
        string username
    )
    {
        // User info is not available in ListenBrainz API
        return new ListenBrainzUserInfo();
    }
}
