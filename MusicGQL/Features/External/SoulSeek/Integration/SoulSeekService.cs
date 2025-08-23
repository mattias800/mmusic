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

    public async Task<DownloadLogger> GetLogger()
    {
        if (serviceLogger == null)
        {
            var path = await new DownloadLogPathProvider(serverSettingsAccessor).GetServiceLogFilePathAsync("soulseek");
            serviceLogger = new DownloadLogger(path);
        }
        return serviceLogger;
    }

    public async Task Connect()
    {
        var serviceLogger = await GetLogger();
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
                serviceLogger.Info($"Connect ignored (state={State.NetworkState}, isConnecting={_isConnecting})");
                return;
            }

            _isConnecting = true;
        }

        await Task.Delay(1000);

        State = new SoulSeekState(NetworkState: SoulSeekNetworkState.Connecting);

        PublishUpdate();
        logger.LogInformation("Connecting to Soulseek...");
        serviceLogger.Info("Connecting to Soulseek...");
        serviceLogger.Info($"Host={options.Value.Host}:{options.Value.Port} Username={options.Value.Username}");

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
            serviceLogger.Info("Connected successfully");

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
            serviceLogger.Error($"Connect failed: {ex.Message}");
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
        var serviceLogger = GetLogger().GetAwaiter().GetResult();
        State = new(SoulSeekNetworkState.Offline);
        PublishUpdate();
        logger.LogWarning(
            "Disconnected from Soulseek: {Message} (Exception={Exception})",
            e.Message,
            e.Exception?.Message
        );
        serviceLogger.Warn($"Disconnected: {e.Message} Exception={e.Exception?.Message}");
    }

    private async void OnConnected(object? sender, EventArgs eventArgs)
    {
        var serviceLogger = await GetLogger();
        State = new(SoulSeekNetworkState.Online);
        PublishUpdate();
        logger.LogInformation("Connected to Soulseek");
        serviceLogger.Info("Connected to Soulseek");
        serviceLogger.Info("Ready: Soulseek network is Online — downloads/searches may proceed");

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
        var serviceLogger = await GetLogger();
        try
        {
            await librarySharingService.PublishSharedCountsAsync();
            await librarySharingService.CheckReachabilityAsync();
            serviceLogger.Info("Published share counts post-login and ran reachability check");
            serviceLogger.Info("Ready: Logged in to Soulseek — downloads/searches may proceed");
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