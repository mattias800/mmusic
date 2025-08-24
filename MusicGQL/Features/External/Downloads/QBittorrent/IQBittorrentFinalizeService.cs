using System.Threading;
using System.Threading.Tasks;

namespace MusicGQL.Features.External.Downloads.QBittorrent;

public interface IQBittorrentFinalizeService
{
    Task<bool> FinalizeReleaseAsync(
        string artistId,
        string releaseFolderName,
        CancellationToken ct
    );
}
