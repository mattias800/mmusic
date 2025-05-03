using Hqub.MusicBrainz;

namespace MusicGQL.Features.Release;

public record ReleaseSearchRoot
{
    public async Task<IEnumerable<Release>> SearchByName([Service] MusicBrainzClient client, string name)
    {
        var artists = await client.Releases.SearchAsync(name, 20);
        return artists.Items.Select(a => new Release(a));
    }

    public async Task<Release?> ById([Service] MusicBrainzClient client, [ID] string id)
    {
        try
        {
            var recording = await client.Releases.GetAsync(id);
            return recording != null ? new Release(recording) : null;
        }
        catch
        {
            return null;
        }
    }
}