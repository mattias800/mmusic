namespace MusicGQL.Features.External.Downloads;

public class DownloadProviderCatalog
{
    private readonly IReadOnlyList<IDownloadProvider> _providers;

    public DownloadProviderCatalog(IEnumerable<IDownloadProvider> providers)
    {
        _providers = providers.ToList();
    }

    public IReadOnlyList<IDownloadProvider> Providers => _providers;
}


