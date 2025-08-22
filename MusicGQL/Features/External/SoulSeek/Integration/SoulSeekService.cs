using HotChocolate.Subscriptions;
using Microsoft.Extensions.Options;
using MusicGQL.Features.ServerSettings;
using Soulseek;
using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.External.SoulSeek.Integration;

public class SoulSeekService(
    SoulseekClient client,
    IOptions<SoulSeekConnectOptions> options,
    ITopicEventSender eventSender,
    ILogger<SoulSeekService> logger,
    ServerSettingsAccessor serverSettingsAccessor,
    SoulSeekLibrarySharingService librarySharingService
)
{
    public SoulSeekState State { get; set; } = new(SoulSeekNetworkState.Offline);
    private DownloadLogger? serviceLogger;
    private readonly object _connectSync = new();
    private bool _eventsHooked = false;
    private bool _isConnecting = false;

    public async Task Connect()
    {
        // Prevent duplicate event subscriptions and concurrent connects
        lock (_connectSync)
        {
            if (!_eventsHooked)
            {
                client.Connected += OnConnected;
                client.Disconnected += OnDisconnected;
                client.LoggedIn += OnLoggedIn;
                _eventsHooked = true;
            }

            if (_isConnecting || State.NetworkState == SoulSeekNetworkState.Connecting || State.NetworkState == SoulSeekNetworkState.Online)
            {
                logger.LogDebug("[SoulSeek] Connect() ignored (state: {State}, isConnecting={IsConnecting})", State.NetworkState, _isConnecting);
                try { serviceLogger?.Info($"Connect ignored (state={State.NetworkState}, isConnecting={_isConnecting})"); } catch { }
                return;
            }

            _isConnecting = true;
        }

        await Task.Delay(1000);

        State = new SoulSeekState(NetworkState: SoulSeekNetworkState.Connecting);

        PublishUpdate();
        logger.LogInformation("Connecting to Soulseek...");
        try
        {
            if (serviceLogger == null)
            {
                var path = await new DownloadLogPathProvider(serverSettingsAccessor).GetServiceLogFilePathAsync("soulseek");
                if (!string.IsNullOrWhiteSpace(path)) serviceLogger = new DownloadLogger(path!);
            }
            serviceLogger?.Info("Connecting to Soulseek...");
            serviceLogger?.Info($"Host={options.Value.Host}:{options.Value.Port} Username={options.Value.Username}");
        }
        catch { }

        try
        {
            // Configure client for sharing (listener + resolvers) before connecting
            await librarySharingService.ConfigureClientAsync();

            await client.ConnectAsync(
                options.Value.Host,
                options.Value.Port,
                options.Value.Username,
                options.Value.Password
            );
            try { serviceLogger?.Info("Connected successfully"); } catch { }

            // Initialize library sharing after successful connection
            await librarySharingService.InitializeAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to connect to Soulseek host {Host}:{Port}",
                options.Value.Host,
                options.Value.Port
            );
            try { serviceLogger?.Error($"Connect failed: {ex.Message}"); } catch { }
            State = new(SoulSeekNetworkState.Offline);
            PublishUpdate();
        }
        finally
        {
            lock (_connectSync)
            {
                _isConnecting = false;
            }
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
        try { serviceLogger?.Warn($"Disconnected: {e.Message} Exception={e.Exception?.Message}"); } catch { }
    }

    private async void OnConnected(object? sender, EventArgs eventArgs)
    {
        State = new(SoulSeekNetworkState.Online);
        PublishUpdate();
        logger.LogInformation("Connected to Soulseek");
        try { serviceLogger?.Info("Connected to Soulseek"); } catch { }
        try { serviceLogger?.Info("Ready: Soulseek network is Online — downloads/searches may proceed"); } catch { }

        // Start sharing the library after connection
        try
        {
            await librarySharingService.StartSharingAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to start library sharing after connection");
        }
    }

    // Subscribe to LoggedIn to publish share counts and run reachability check
    private async void OnLoggedIn(object? sender, EventArgs e)
    {
        try
        {
            await librarySharingService.PublishSharedCountsAsync();
            await librarySharingService.CheckReachabilityAsync();
            try { serviceLogger?.Info("Published share counts post-login and ran reachability check"); } catch { }
            try { serviceLogger?.Info("Ready: Logged in to Soulseek — downloads/searches may proceed"); } catch { }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Post-login share publish/reachability failed");
        }
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
}