using HotChocolate;
using MusicGQL.Integration.ListenBrainz;
using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.ListenBrainz;

public record ListenBrainzQueryRoot
{
    public async Task<ListenBrainzUserInfo?> GetUserInfo(
        [Service] ListenBrainzService listenBrainzService,
        [Service] ServerSettingsAccessor serverSettingsAccessor,
        string username
    )
    {
        var settings = await serverSettingsAccessor.GetAsync();
        if (string.IsNullOrEmpty(settings.ListenBrainzUsername) || 
            string.IsNullOrEmpty(settings.ListenBrainzApiKey))
        {
            return null;
        }

        // User info is not available in ListenBrainz API
        return new ListenBrainzUserInfo();
    }
}
