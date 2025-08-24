using HotChocolate;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Integration.ListenBrainz;

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
