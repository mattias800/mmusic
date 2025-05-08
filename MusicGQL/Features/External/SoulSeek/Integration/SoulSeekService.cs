using HotChocolate.Subscriptions;
using Microsoft.Extensions.Options;
using Soulseek;

namespace MusicGQL.Features.External.SoulSeek.Integration;

public class SoulSeekService(
    ISoulseekClient client,
    IOptions<SoulSeekConnectOptions> options,
    ITopicEventSender eventSender,
    ILogger<SoulSeekService> logger
)
{
    public SoulSeekState State { get; private set; } = new(SoulSeekNetworkState.Offline);

    public async Task Connect()
    {
        client.Connected += OnConnected;
        client.Disconnected += OnDisconnected;

        await Task.Delay(1000);

        State = new SoulSeekState(NetworkState: SoulSeekNetworkState.Connecting);

        PublishUpdate();
        logger.LogInformation("Connecting to Soulseek...");

        await client.ConnectAsync(
            options.Value.Host,
            options.Value.Port,
            options.Value.Username,
            options.Value.Password
        );
    }

    private void OnDisconnected(object? sender, SoulseekClientDisconnectedEventArgs e)
    {
        State = new(SoulSeekNetworkState.Offline);
        PublishUpdate();
        logger.LogInformation("Disconnected from Soulseek: {Reason}", e.Message);
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
