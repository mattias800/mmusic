using HotChocolate;
using MusicGQL.Integration.ListenBrainz;
using MetaBrainz.ListenBrainz.Objects;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Types;

namespace MusicGQL.Features.ListenBrainz;

public record SubmitListenInput(
    string TrackName,
    string ArtistName,
    string? AlbumName = null,
    DateTime? ListenedAt = null
);

[UnionType]
public abstract record SubmitListenResult;

public record SubmitListenSuccess : SubmitListenResult
{
    public bool Success { get; } = true;
}

public record SubmitListenError(string Message) : SubmitListenResult;

[ExtendObjectType(typeof(Mutation))]
public record ListenBrainzMutations
{
    public async Task<SubmitListenResult> SubmitListen(
        SubmitListenInput input,
        [Service] ListenBrainzService listenBrainzService
    )
    {
        try
        {
            var listen = new SubmittedListen(
                input.ListenedAt ?? DateTime.UtcNow,
                input.TrackName,
                input.ArtistName,
                input.AlbumName ?? ""
            );

            var success = await listenBrainzService.SubmitSingleListenAsync(listen);
            
            if (success)
            {
                return new SubmitListenSuccess();
            }
            else
            {
                return new SubmitListenError("Failed to submit listen to ListenBrainz");
            }
        }
        catch (Exception ex)
        {
            return new SubmitListenError($"Error submitting listen: {ex.Message}");
        }
    }
}
