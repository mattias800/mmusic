using HotChocolate.Subscriptions;
using Microsoft.Extensions.Options;
using MusicGQL.Features.ServerSettings;
using Soulseek;
using System.IO;
using System.Linq;

namespace MusicGQL.Features.External.SoulSeek.Integration;

public class SoulSeekService(
    ISoulseekClient client,
    IOptions<SoulSeekConnectOptions> options,
    ITopicEventSender eventSender,
    ILogger<SoulSeekService> logger,
    ServerSettingsAccessor serverSettingsAccessor
)
{
    public SoulSeekState State { get; set; } = new(SoulSeekNetworkState.Offline);

    public async Task Connect()
    {
        client.Connected += OnConnected;
        client.Disconnected += OnDisconnected;

        await Task.Delay(1000);

        State = new SoulSeekState(NetworkState: SoulSeekNetworkState.Connecting);

        PublishUpdate();
        logger.LogInformation("Connecting to Soulseek...");

        try
        {
            await client.ConnectAsync(
                options.Value.Host,
                options.Value.Port,
                options.Value.Username,
                options.Value.Password
            );

            // Attempt to configure shared directories to only the server library path
            await TryConfigureSharesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to connect to Soulseek host {Host}:{Port}",
                options.Value.Host,
                options.Value.Port
            );
            State = new(SoulSeekNetworkState.Offline);
            PublishUpdate();
        }
    }

    private void OnDisconnected(object? sender, SoulseekClientDisconnectedEventArgs e)
    {
        State = new(SoulSeekNetworkState.Offline);
        PublishUpdate();
        logger.LogWarning(
            "Disconnected from Soulseek: {Message} (Exception={Exception})",
            e.Message,
            e.Exception?.Message
        );
    }

    private void OnConnected(object? sender, EventArgs eventArgs)
    {
        State = new(SoulSeekNetworkState.Online);
        PublishUpdate();
        logger.LogInformation("Connected to Soulseek");
        _ = TryConfigureSharesAsync();
    }

    private void PublishUpdate()
    {
        var stateSnapshot = new SoulSeekState(State.NetworkState);

        logger.LogInformation(
            "Publishing Soulseek status update: {Status}",
            stateSnapshot.NetworkState
        );

        _ = eventSender.SendAsync(
            nameof(SoulSeekSubscription.SoulSeekStatusUpdated),
            stateSnapshot
        );
    }

    private async Task TryConfigureSharesAsync()
    {
        try
        {
            var settings = await serverSettingsAccessor.GetAsync();
            var libraryPath = settings.LibraryPath;

            if (string.IsNullOrWhiteSpace(libraryPath))
            {
                logger.LogWarning("[SoulSeek] LibraryPath is not set in server settings; skipping share configuration.");
                return;
            }

            if (!System.IO.Directory.Exists(libraryPath))
            {
                logger.LogWarning("[SoulSeek] LibraryPath '{LibraryPath}' does not exist; skipping share configuration.", libraryPath);
                return;
            }

            // Prefer async method names commonly used in clients; fall back gracefully via reflection
            var clientType = client.GetType();

            // Candidate method names that accept a string[] of directories
            var methodNames = new[]
            {
                "SetSharedDirectoriesAsync",
                "SetSharedFoldersAsync",
                "SetSharesAsync",
                "ConfigureSharesAsync"
            };

            var setSharesMethod = methodNames
                .Select(name => clientType.GetMethod(name, new[] { typeof(string[]) }))
                .FirstOrDefault(m => m != null);

            if (setSharesMethod != null)
            {
                await (Task)setSharesMethod.Invoke(client, new object[] { new[] { libraryPath } })!;
                logger.LogInformation("[SoulSeek] Configured shared directories to only: {LibraryPath}", libraryPath);
                return;
            }

            // Some clients may expose a non-async variant
            var setSharesSyncMethod = methodNames
                .Select(name => clientType.GetMethod(name.Replace("Async", string.Empty), new[] { typeof(string[]) }))
                .FirstOrDefault(m => m != null);

            if (setSharesSyncMethod != null)
            {
                setSharesSyncMethod.Invoke(client, new object[] { new[] { libraryPath } });
                logger.LogInformation("[SoulSeek] Configured shared directories to only: {LibraryPath}", libraryPath);
                return;
            }

            // If the library exposes a rescan or refresh shares method, try calling it after setting env/config elsewhere
            var rescanMethod = clientType.GetMethod("RescanSharesAsync", Type.EmptyTypes) ?? clientType.GetMethod("RescanShares", Type.EmptyTypes);
            if (rescanMethod != null)
            {
                var result = rescanMethod.Invoke(client, Array.Empty<object>());
                if (result is Task task) await task;
                logger.LogInformation("[SoulSeek] Invoked rescan of shared directories.");
            }

            logger.LogWarning("[SoulSeek] Could not find a supported method to configure shared directories. Please ensure the client library supports programmatic share configuration.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[SoulSeek] Failed to configure shared directories to library path.");
        }
    }
}
