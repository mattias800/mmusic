using Google.Apis.Services;
using Hqub.Lastfm;
using Hqub.MusicBrainz;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models.Projections;
using MusicGQL.EventProcessor;
using MusicGQL.Features.Authentication.Handlers;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.Downloads.Mutations;
using MusicGQL.Features.Downloads.Sagas;
using MusicGQL.Features.Downloads.Sagas.Handlers;
using MusicGQL.Features.External.SoulSeek;
using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Features.LikedSongs.Aggregate;
using MusicGQL.Features.LikedSongs.Commands;
using MusicGQL.Features.LikedSongs.Mutations;
using MusicGQL.Features.Playlists.Import.Spotify;
using MusicGQL.Features.Playlists.Import.Spotify.Mutations;
using MusicGQL.Features.ServerLibrary.Artist;
using MusicGQL.Features.ServerLibrary.Artist.Aggregate;
using MusicGQL.Features.ServerLibrary.Artist.Handlers;
using MusicGQL.Features.ServerLibrary.Artist.Mutations;
using MusicGQL.Features.ServerLibrary.ArtistServerStatus;
using MusicGQL.Features.ServerLibrary.ArtistServerStatus.Services;
using MusicGQL.Features.ServerLibrary.Import.Handlers;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Aggregate;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Handlers;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Mutations;
using MusicGQL.Features.Users.Aggregate;
using MusicGQL.Features.Users.Handlers;
using MusicGQL.Features.Users.Mutations;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;
using MusicGQL.Integration.Spotify;
using MusicGQL.Integration.Spotify.Configuration;
using MusicGQL.Integration.Youtube.Configuration;
using MusicGQL.Types;
using Neo4j.Driver;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Soulseek;
using Soulseek.Diagnostics;
using SpotifyAPI.Web;
using StackExchange.Redis;
using YouTubeService = MusicGQL.Integration.Youtube.YouTubeService;

var builder = WebApplication.CreateBuilder(args);

// Configure YouTubeServiceOptions
builder.Services.Configure<YouTubeServiceOptions>(
    builder.Configuration.GetSection(YouTubeServiceOptions.SectionName)
);

// Configure SpotifyClientOptions
builder.Services.Configure<SpotifyClientOptions>(
    builder.Configuration.GetSection(SpotifyClientOptions.SectionName)
);

builder.Services.Configure<SoulSeekConnectOptions>(builder.Configuration.GetSection("SoulSeek"));

builder
    .Services.AddHybridCache(options =>
    {
        options.MaximumPayloadBytes = 10485760; // 10 MB
    })
    .Services.AddSingleton<IMemoryCache, MemoryCache>()
    .AddSingleton<ISoulseekClient, SoulseekClient>(_ => new SoulseekClient(
        options: new SoulseekClientOptions(
            maximumConcurrentUploads: 5,
            maximumConcurrentDownloads: 5,
            minimumDiagnosticLevel: DiagnosticLevel.Debug
        )
    ))
    .AddSingleton<ServerLibraryImporterService>()
    .AddSingleton<ServerLibraryService>()
    .AddSingleton<SoulSeekService>()
    .AddSingleton<MusicBrainzService>()
    .AddSingleton<YouTubeService>()
    .AddSingleton<SpotifyService>()
    .AddSingleton<ArtistServerStatusService>()
    .AddScoped<LikeSongHandler>()
    .AddScoped<UnlikeSongHandler>()
    .AddScoped<ImportArtistToServerLibraryHandler>()
    .AddScoped<ImportReleaseGroupToServerLibraryHandler>()
    .AddScoped<MarkReleaseGroupAsAddedToServerLibraryHandler>()
    .AddScoped<MarkArtistAsAddedToServerLibraryHandler>()
    .AddScoped<MarkArtistReleaseGroupsAsAddedToServerLibraryHandler>()
    .AddScoped<ProcessMissingMetaDataHandler>()
    .AddScoped<MissingMetaDataProcessingService>()
    // Event processors
    .AddScoped<LikedSongsEventProcessor>()
    .AddScoped<ReleaseGroupsAddedToServerLibraryProcessor>()
    .AddScoped<ArtistsAddedToServerLibraryProcessor>()
    .AddScoped<UserEventProcessor>()
    .AddScoped<EventProcessorWorker>()
    // Register new Handlers
    .AddScoped<HashPasswordHandler>()
    .AddScoped<VerifyPasswordHandler>()
    .AddScoped<CreateUserHandler>()
    // Register YouTubeService
    .AddSingleton<Google.Apis.YouTube.v3.YouTubeService>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<YouTubeServiceOptions>>().Value;
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException("YouTube API key is not configured.");
        }

        return new Google.Apis.YouTube.v3.YouTubeService(
            new BaseClientService.Initializer
            {
                ApiKey = options.ApiKey,
                ApplicationName = options.ApplicationName ?? "MusicGQL",
            }
        );
    });

// Add Spotify Client
builder.Services.AddSingleton<SpotifyClient>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<SpotifyClientOptions>>().Value;

    if (string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.ClientSecret))
    {
        throw new InvalidOperationException("Spotify ClientId or ClientSecret is not configured.");
    }

    var spotifyConfig = SpotifyClientConfig.CreateDefault();
    var request = new ClientCredentialsRequest(options.ClientId, options.ClientSecret);
    // It's generally not recommended to block on async calls in this manner in a synchronous context.
    // However, during application startup, this is often acceptable.
    // Consider if a fully async setup is possible or more appropriate for your application's needs.
    var response = new OAuthClient(spotifyConfig).RequestToken(request).GetAwaiter().GetResult();

    return new SpotifyClient(spotifyConfig.WithToken(response.AccessToken));
});

builder.Services.AddSingleton(GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.None));

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
);

// Add Authentication services
builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // Path to the login page UI (to be created)
        options.LogoutPath = "/logout";
        // Further options can be configured here, like cookie expiration.
    });
builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpContextAccessor();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder
    .AddGraphQL()
    .ModifyRequestOptions(o =>
    {
        o.ExecutionTimeout = TimeSpan.FromSeconds(60);
    })
    .AddRedisSubscriptions(
        (_) =>
            ConnectionMultiplexer.Connect(
                builder.Configuration.GetConnectionString("Redis") ?? string.Empty
            )
    )
    .AddDiagnosticEventListener<MyExecutionEventListener>()
    .AddQueryType<MusicGQL.Types.Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddTypeExtension<SpotifyPlaylistSearchRoot>()
    .AddTypeExtension<ImportSpotifyPlaylistMutation>()
    .AddType<ImportSpotifyPlaylistSuccess>()
    .AddType<ImportSpotifyPlaylistError>()
    .AddTypeExtension<SoulSeekSubscription>()
    .AddTypeExtension<DownloadSubscription>()
    .AddTypeExtension<ArtistServerStatusSubscription>()
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
    .AddType<AddArtistToServerLibraryResult.AddArtistToServerLibraryUnknownError>()
    .AddTypeExtension<CreateUserMutation>()
    .AddType<CreateUserSuccess>()
    .AddType<CreateUserError>()
    .AddType<UserProjection>()
    .AddTypeExtension<SignInMutation>()
    .AddType<SignInSuccess>()
    .AddType<SignInError>()
    .AddTypeExtension<SignOutMutation>()
    .AddType<SignOutSuccess>()
    .AddType<SignOutError>()
    .AddType<ArtistServerStatusReady>()
    .AddType<ArtistServerStatusImportingArtist>()
    .AddType<ArtistServerStatusUpdatingArtist>()
    .AddType<ArtistServerStatusImportingArtistReleases>()
    .AddType<ArtistServerStatusUpdatingArtistReleases>()
    .AddType<ArtistServerStatusNotInLibrary>()
    .AddType<IArtistServerStatusResult>()
    .AddType<IArtistBase>();

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

builder.Services.AddHostedService<ScheduledTaskPublisher>();

var app = builder.Build();

// Ensure Neo4j constraints are created/verified on startup
await Neo4JSchemaSetup.EnsureConstraintsAsync(app.Services, app.Logger);

app.UseRouting(); // Ensure UseRouting is called before UseAuthentication and UseAuthorization

app.UseAuthentication();
app.UseAuthorization();

// 🟢 Run event processor once on startup
using (var scope = app.Services.CreateScope())
{
    var processor = scope.ServiceProvider.GetRequiredService<EventProcessorWorker>();
    await processor.ProcessEvents();

    var soulSeekService = scope.ServiceProvider.GetRequiredService<SoulSeekService>();
    // _ = soulSeekService.Connect();

    var missingMetaDataProcessingService =
        scope.ServiceProvider.GetRequiredService<MissingMetaDataProcessingService>();
    missingMetaDataProcessingService.ProcessMissingMetaData();
}

app.UseWebSockets();

app.MapGraphQL();

app.Run();
//app.RunWithGraphQLCommands(args);
