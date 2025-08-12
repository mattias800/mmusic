namespace MusicGQL.Features.Downloads.Mutations;

[ExtendObjectType(typeof(Types.Mutation))]
public sealed class DownloadQueueMutations
{
    public bool RemoveDownloadJob(
        string queueKey,
        [Service] Services.DownloadQueueService queue
    )
    {
        return queue.TryRemove(queueKey);
    }
}


