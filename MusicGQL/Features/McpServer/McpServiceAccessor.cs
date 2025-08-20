using Microsoft.Extensions.DependencyInjection;

namespace MusicGQL.Features.McpServer;

public static class McpServiceAccessor
{
    private static IServiceProvider? _services;
    public static IServiceProvider Services => _services ?? throw new InvalidOperationException("MCP services not initialized");

    public static void Initialize(IServiceProvider services)
    {
        _services = services;
    }
}


