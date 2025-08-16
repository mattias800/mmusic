using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Users.Events;

public class UserListenBrainzCredentialsUpdated : Event
{
    public Guid SubjectUserId { get; init; }
    public string? ListenBrainzUserId { get; init; }
    public string? ListenBrainzToken { get; init; }

    public UserListenBrainzCredentialsUpdated(Guid subjectUserId, string? listenBrainzUserId, string? listenBrainzToken)
    {
        SubjectUserId = subjectUserId;
        ListenBrainzUserId = listenBrainzUserId;
        ListenBrainzToken = listenBrainzToken;
    }
}
