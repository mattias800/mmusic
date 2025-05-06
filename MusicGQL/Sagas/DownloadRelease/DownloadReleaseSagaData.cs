using Hqub.MusicBrainz.Entities;
using Rebus.Sagas;

namespace MusicGQL.Sagas.DownloadRelease;

public class DownloadReleaseSagaData : ISagaData
{
    public Guid Id { get; set; }
    public int Revision { get; set; }
    
    // Update these to our use-case
    public string MusicBrainzReleaseId { get; set; }
    public Release? Release { get; set; }
}