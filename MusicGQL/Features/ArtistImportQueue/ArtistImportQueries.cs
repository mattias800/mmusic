using MusicGQL.Types;

namespace MusicGQL.Features.ArtistImportQueue;

[ExtendObjectType(typeof(Query))]
public sealed class ArtistImportQueries
{
    public ArtistImportQueueState ArtistImportQueue([Service] ArtistImportQueueService queue) => queue.Snapshot();

    public ArtistImportProgress CurrentArtistImport([Service] CurrentArtistImportStateService state) => state.Get();
}


