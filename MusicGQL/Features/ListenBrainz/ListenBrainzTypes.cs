using HotChocolate;
using MusicGQL.Integration.ListenBrainz;

namespace MusicGQL.Features.ListenBrainz;

public record ListenBrainzUserInfo
{
    [ID]
    public string Id() => $"listenbrainz:user_info:placeholder";

    public string Username() => "Not Available";

    public string? Bio() => null;

    public string? Location() => null;

    public DateTime? Joined() => null;

    public string? Website() => null;
}
