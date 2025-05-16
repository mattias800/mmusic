using Hqub.Lastfm;
using Hqub.MusicBrainz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicGQL.Db;
using MusicGQL.EventProcessor;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.Downloads.Mutations;
using MusicGQL.Features.External.SoulSeek;
using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Features.LikedSongs.Aggregate;
using MusicGQL.Features.LikedSongs.Commands;
using MusicGQL.Features.LikedSongs.Mutations;
using MusicGQL.Features.ServerLibrary.Artist.Aggregate;
using MusicGQL.Features.ServerLibrary.Artist.Handlers;
using MusicGQL.Features.ServerLibrary.Artist.Mutations;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Aggregate;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Mutations;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Sagas.DownloadRelease;
using MusicGQL.Sagas.DownloadRelease.Handlers;
using MusicGQL.Types;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Soulseek;
using Soulseek.Diagnostics;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SoulSeekConnectOptions>(builder.Configuration.GetSection("SoulSeek"));

builder
    .Services.AddHybridCache()
    .Services.AddSingleton<IMemoryCache, MemoryCache>()
    .AddSingleton<ISoulseekClient, SoulseekClient>(_ => new SoulseekClient(
        options: new SoulseekClientOptions(
            maximumConcurrentUploads: 5,
            maximumConcurrentDownloads: 5,
            minimumDiagnosticLevel: DiagnosticLevel.Debug
        )
    ))
    .AddSingleton<SoulSeekService>()
    .AddSingleton<MusicBrainzService>()
    .AddScoped<LikeSongHandler>()
    .AddScoped<UnlikeSongHandler>()
    .AddScoped<AddReleaseGroupToServerLibraryHandler>()
    .AddScoped<AddArtistToServerLibraryHandler>()
    // Event processors
    .AddScoped<LikedSongsEventProcessor>()
    .AddScoped<ReleaseGroupsAddedToServerLibraryProcessor>()
    .AddScoped<ArtistsAddedToServerLibraryProcessor>()
    .AddScoped<EventProcessorWorker>();

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder
    .AddGraphQL()
    .AddRedisSubscriptions(
        (sp) =>
            ConnectionMultiplexer.Connect(
                builder.Configuration.GetConnectionString("Redis") ?? string.Empty
            )
    )
    .AddDiagnosticEventListener<MyExecutionEventListener>()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddTypeExtension<SoulSeekSubscription>()
    .AddTypeExtension<DownloadSubscription>()
    .AddTypeExtension<StartDownloadReleaseMutation>()
    .AddType<StartDownloadReleaseSuccess>()
    .AddTypeExtension<UnlikeSongMutation>()
    .AddTypeExtension<LikeSongMutation>()
    .AddType<LikeSongResult.LikeSongSuccess>()
    .AddType<LikeSongResult.LikeSongAlreadyLiked>()
    .AddType<LikeSongResult.LikeSongSongDoesNotExist>()
    .AddTypeExtension<AddReleaseGroupToServerLibraryMutation>()
    .AddType<AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibrarySuccess>()
    .AddType<AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibraryReleaseGroupAlreadyAdded>()
    .AddType<AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibraryReleaseGroupDoesNotExist>()
    .AddType<AddReleaseGroupToServerLibraryResult.AddReleaseGroupToServerLibraryUnknownError>()
    .AddTypeExtension<AddArtistToServerLibraryMutation>()
    .AddType<AddArtistToServerLibraryResult.AddArtistToServerLibrarySuccess>()
    .AddType<AddArtistToServerLibraryResult.AddArtistToServerLibraryArtistAlreadyAdded>()
    .AddType<AddArtistToServerLibraryResult.AddArtistToServerLibraryArtistDoesNotExist>()
    .AddType<AddArtistToServerLibraryResult.AddArtistToServerLibraryUnknownError>();

builder.Services.Configure<LastfmOptions>(builder.Configuration.GetSection("Lastfm"));

builder.Services.AddSingleton<LastfmClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<LastfmOptions>>().Value;
    return new LastfmClient(options.ApiKey);
});

builder.Services.AddHttpClient<MusicBrainzClient>(client =>
{
    client.BaseAddress = new Uri("https://musicbrainz.org/ws/2/");
    client.DefaultRequestHeaders.Add(
        "User-Agent",
        "Hqub.MusicBrainz/3.0 (https://github.com/avatar29A/MusicBrainz)"
    );
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddFanArtTVClient(options =>
{
    var fanartOptions = builder.Configuration.GetSection("Fanart").Get<FanartOptions>();
    options.ApiKey = fanartOptions?.ApiKey;
    options.BaseAddress = fanartOptions?.BaseAddress;
});

builder.Services.AddRebus(
    rebus =>
        rebus
            .Routing(r =>
                r.TypeBased()
                    .Map<DownloadReleaseQueuedEvent>("mmusic-queue")
                    .Map<LookupReleaseInMusicBrainz>("mmusic-queue")
                    .Map<LookupRecordingsForReleaseInMusicBrainz>("mmusic-queue")
                    .Map<FoundReleaseInMusicBrainz>("mmusic-queue")
                    .Map<FoundRecordingsForReleaseInMusicBrainz>("mmusic-queue")
                    .Map<ReleaseNotFoundInMusicBrainz>("mmusic-queue")
                    .Map<NoRecordingsFoundInMusicBrainz>("mmusic-queue")
            )
            .Transport(t =>
                t.UseRabbitMq(
                    builder.Configuration.GetConnectionString("MessageBroker"),
                    "mmusic-queue"
                )
            )
            // Use JSON serialization so EF can query the database
            .Sagas(s =>
                s.StoreInPostgres(
                    builder.Configuration.GetConnectionString("Postgres"),
                    "sagas",
                    "saga_indexes"
                )
            ),
    onCreated: async bus =>
    {
        await bus.Subscribe<FoundReleaseInMusicBrainz>();
    }
);

builder.Services.AddRebusHandler<DownloadReleaseSaga>();
builder.Services.AddRebusHandler<LookupReleaseInMusicBrainzHandler>();
builder.Services.AddRebusHandler<LookupRecordingsForReleaseInMusicBrainzHandler>();

var app = builder.Build();

// 🟢 Run event processor once on startup
using (var scope = app.Services.CreateScope())
{
    var processor = scope.ServiceProvider.GetRequiredService<EventProcessorWorker>();
    await processor.ProcessEvents();

    var soulSeekService = scope.ServiceProvider.GetRequiredService<SoulSeekService>();
    _ = soulSeekService.Connect();
}

app.UseRouting();

app.UseWebSockets();

app.MapGraphQL();

app.Run();
//app.RunWithGraphQLCommands(args);
