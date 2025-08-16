using HotChocolate.Subscriptions;
using Microsoft.Extensions.Options;
using MusicGQL.Features.ServerSettings;
using Soulseek;

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