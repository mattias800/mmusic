using Hqub.MusicBrainz;

namespace MusicGQL.Features.Recording;

public record RecordingSearchRoot
{
    public async Task<IEnumerable<Recording>> SearchByName([Service] MusicBrainzClient client, string name)
    {
        var artists = await client.Recordings.SearchAsync(name, 20);
        return artists.Items.Select(a => new Recording(a));
    }

    public async Task<Recording?> ById([Service] MusicBrainzClient client, [ID] string id)
    {
        var recording = await client.Recordings.GetAsync(id);
        return recording != null ? new Recording(recording) : null;
    }
}