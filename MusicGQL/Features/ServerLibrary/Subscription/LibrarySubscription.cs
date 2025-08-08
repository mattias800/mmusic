namespace MusicGQL.Features.ServerLibrary.Subscription;

[ExtendObjectType(typeof(MusicGQL.Types.Subscription))]
public record LibrarySubscription
{
    [Subscribe]
    public LibraryCacheTrackUpdated LibraryCacheTrackUpdated(
        [EventMessage] LibraryCacheTrackUpdated u
    ) => u;
}

public record LibraryCacheTrackUpdated(Track Track);
