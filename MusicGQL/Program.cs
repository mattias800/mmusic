using Google.Apis.Services;
using Hqub.Lastfm;
using Hqub.MusicBrainz;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicGQL.Db.Postgres;
using MusicGQL.EventProcessor;
using MusicGQL.Features.ArtistServerStatus;
using MusicGQL.Features.ArtistServerStatus.Services;
using MusicGQL.Features.Authentication.Handlers;
using MusicGQL.Features.Authorization;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.Downloads.Mutations;
using MusicGQL.Features.Downloads.Sagas;
using MusicGQL.Features.Downloads.Sagas.Handlers;
using MusicGQL.Features.External.SoulSeek;
using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Features.FileSystem;
using MusicGQL.Features.FileSystem.Mutations;
using MusicGQL.Features.Import;
using MusicGQL.Features.Import.Handlers;
using MusicGQL.Features.Import.Mutations;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.Likes.Commands;
using MusicGQL.Features.Likes.Events;
using MusicGQL.Features.Likes.Mutations;
using MusicGQL.Features.Playlists.Aggregate;
using MusicGQL.Features.Playlists.Commands;
using MusicGQL.Features.Playlists.Import.Spotify;
using MusicGQL.Features.Playlists.Import.Spotify.Mutations;
using MusicGQL.Features.Playlists.Mutations;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Reader;
using MusicGQL.Features.ServerSettings.Commands;
using MusicGQL.Features.ServerSettings.Events;
using MusicGQL.Features.ServerSettings.Mutations;
using MusicGQL.Features.Users.Aggregate;
using MusicGQL.Features.Users.Handlers;
using MusicGQL.Features.Users.Mutations;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Spotify;
using MusicGQL.Integration.Spotify.Configuration;
using MusicGQL.Integration.Youtube.Configuration;
using MusicGQL.Types;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Soulseek;
using Soulseek.Diagnostics;
using SpotifyAPI.Web;
using StackExchange.Redis;
using YouTubeService = MusicGQL.Integration.Youtube.YouTubeService;

var builder = WebApplication.CreateBuilder(args);

// Cookie settings for module federation.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "MusicGQL.AspNetCore.Session";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.HttpOnly = true;
});

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
    .AddSingleton<SoulSeekService>()
    .AddSingleton<MusicBrainzService>()
    .AddSingleton<YouTubeService>()
    .AddSingleton<SpotifyService>()
    .AddSingleton<ArtistServerStatusService>()
    .AddSingleton<ServerLibraryJsonReader>()
    .AddSingleton<ServerLibraryAssetReader>()
    .AddSingleton<ServerLibraryFileSystemScanner>()
    .AddSingleton<ServerLibraryCache>()
    .AddScoped<MusicBrainzImportService>()
    .AddScoped<SpotifyImportService>()
    .AddScoped<FanArtDownloadService>()
    .AddScoped<IFolderIdentityService, FolderIdentityService>()
    .AddScoped<IImportExecutor, MusicBrainzImportExecutor>()
    .AddScoped<LibraryMaintenanceCoordinator>()
    .AddScoped<LibraryImportService>()
    .AddScoped<LikeSongHandler>()
    .AddScoped<UnlikeSongHandler>()
    .AddScoped<UpdateLibraryPathHandler>()
    .AddScoped<UpdateDownloadPathHandler>()
    .AddScoped<ImportArtistToServerLibraryHandler>()
    .AddScoped<ImportArtistReleaseGroupsToServerLibraryHandler>()
    .AddScoped<ImportReleaseGroupToServerLibraryHandler>()
    // Event processors
    .AddScoped<LikedSongsEventProcessor>()
    .AddScoped<UserEventProcessor>()
    .AddScoped<PlaylistsEventProcessor>()
    .AddScoped<ServerSettingsEventProcessor>()
    .AddScoped<EventProcessorWorker>()
    // Register new Handlers
    .AddScoped<HashPasswordHandler>()
    .AddScoped<VerifyPasswordHandler>()
    .AddScoped<CreateUserHandler>()
    .AddScoped<CreatePlaylistHandler>()
    .AddScoped<RenamePlaylistHandler>()
    .AddScoped<DeletePlaylistHandler>()
    .AddScoped<VerifyPlaylistWriteAccessHandler>()
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

builder.Services.AddDbContextFactory<EventDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
);

// Add Authentication services
builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MusicGQL.Authentication";
        options.LoginPath = "/login"; // Path to the login page UI (to be created)
        options.LogoutPath = "/logout";
        // Further options can be configured here, like cookie expiration.
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        b =>
        {
            b.WithOrigins("http://localhost:3000", "localhost:3100")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "IsAuthenticatedUser",
        policy => policy.Requirements.Add(new UserExistsRequirement())
    );
});

builder.Services.AddSingleton<
    Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    UserExistsHandler
>();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpContextAccessor();

// Add MVC controllers for asset endpoints
builder.Services.AddControllers();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder
    .AddGraphQL()
    .AddAuthorization()
    .ModifyRequestOptions(o =>
    {
        o.ExecutionTimeout = TimeSpan.FromSeconds(60);
    })
    .AddRedisSubscriptions(_ =>
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
    .AddTypeExtension<ImportArtistMutation>()
    .AddType<LikeSongResult.LikeSongSuccess>()
    .AddType<LikeSongResult.LikeSongAlreadyLiked>()
    .AddType<LikeSongResult.LikeSongSongDoesNotExist>()
    .AddTypeExtension<UpdateLibraryPathMutation>()
    .AddTypeExtension<UpdateDownloadPathMutation>()
    .AddTypeExtension<MusicGQL.Features.ServerLibrary.ServerLibraryMaintenanceMutation>()
    .AddType<UpdateLibraryPathResult.UpdateLibraryPathSuccess>()
    .AddType<UpdateDownloadPathResult.UpdateDownloadPathSuccess>()
    .AddTypeExtension<AddArtistToServerLibraryMutation>()
    .AddType<AddArtistToServerLibraryResult.AddArtistToServerLibrarySuccess>()
    .AddType<AddArtistToServerLibraryResult.AddArtistToServerLibraryArtistAlreadyAdded>()
    .AddType<AddArtistToServerLibraryResult.AddArtistToServerLibraryArtistDoesNotExist>()
    .AddType<AddArtistToServerLibraryResult.AddArtistToServerLibraryUnknownError>()
    .AddTypeExtension<CreateUserMutation>()
    .AddType<CreateUserSuccess>()
    .AddType<CreateUserError>()
    .AddTypeExtension<SignInMutation>()
    .AddType<SignInSuccess>()
    .AddType<SignInError>()
    .AddTypeExtension<SignOutMutation>()
    .AddType<SignOutSuccess>()
    .AddType<SignOutError>()
    .AddTypeExtension<CreatePlaylistMutation>()
    .AddType<CreatePlaylistSuccess>()
    .AddTypeExtension<RenamePlaylistMutation>()
    .AddType<RenamePlaylistSuccess>()
    .AddType<RenamePlaylistNoWriteAccess>()
    .AddTypeExtension<DeletePlaylistMutation>()
    .AddType<DeletePlaylistSuccess>()
    .AddType<DeletePlaylistNoWriteAccess>()
    .AddType<ArtistServerStatusReady>()
    .AddType<ArtistServerStatusImportingArtist>()
    .AddType<ArtistServerStatusUpdatingArtist>()
    .AddType<ArtistServerStatusImportingArtistReleases>()
    .AddType<ArtistServerStatusUpdatingArtistReleases>()
    .AddType<ArtistServerStatusNotInLibrary>()
    .AddType<IArtistServerStatusResult>()
    .AddTypeExtension<FileSystemSearchRoot>()
    .AddTypeExtension<CreateDirectoryMutation>();

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
app.UseCors("AllowFrontend");
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

    // 🎵 Initialize and populate the music library cache
    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════");
    Console.WriteLine("🎵 INITIALIZING MUSIC LIBRARY CACHE");
    Console.WriteLine("═══════════════════════════════════════════════");

    var cache = scope.ServiceProvider.GetRequiredService<ServerLibraryCache>();

    try
    {
        Console.WriteLine("📀 Loading music library from disk...");
        Console.WriteLine(
            $"   🔍 Looking for library at: {System.IO.Path.GetFullPath("./Library/")}"
        );
        await cache.UpdateCacheAsync();

        var stats = await cache.GetCacheStatisticsAsync();
        Console.WriteLine($"✅ Cache loaded successfully!");
        Console.WriteLine(
            $"   📊 Statistics: {stats.ArtistCount} artists, {stats.ReleaseCount} releases, {stats.TrackCount} tracks"
        );
        Console.WriteLine($"   🕒 Last updated: {stats.LastUpdated:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine();

        // Display all artists and their albums
        var artists = await cache.GetAllArtistsAsync();

        if (artists.Count > 0)
        {
            Console.WriteLine("🎤 ARTISTS & ALBUMS IN LIBRARY:");
            Console.WriteLine("───────────────────────────────────────────────");

            foreach (var artist in artists.OrderBy(a => a.Name))
            {
                Console.WriteLine($"🎸 {artist.Name}");
                if (!string.IsNullOrEmpty(artist.SortName) && artist.SortName != artist.Name)
                {
                    Console.WriteLine($"   (Sort: {artist.SortName})");
                }

                if (artist.Releases.Count > 0)
                {
                    foreach (var release in artist.Releases.OrderBy(r => r.Title))
                    {
                        var typeIcon = release.Type switch
                        {
                            JsonReleaseType.Album => "💿",
                            JsonReleaseType.Ep => "💽",
                            JsonReleaseType.Single => "🎵",
                            _ => "📀",
                        };

                        var trackCountText =
                            release.Tracks.Count > 0 ? $" ({release.Tracks.Count} tracks)" : "";
                        Console.WriteLine($"   {typeIcon} {release.Title}{trackCountText}");

                        if (!string.IsNullOrEmpty(release.JsonRelease.FirstReleaseYear))
                        {
                            Console.WriteLine(
                                $"      📅 Released: {release.JsonRelease.FirstReleaseYear}"
                            );
                        }
                    }
                }
                else
                {
                    Console.WriteLine("   📭 No releases found");
                }

                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("📭 No artists found in library");
            Console.WriteLine("   💡 Make sure your library folder contains artist.json files");
        }

        // After initial cache load, run maintenance: scan, identify, import, and refresh cache again
        Console.WriteLine("🔎 Scanning library for folders with audio but missing JSON...");
        var coordinator = scope.ServiceProvider.GetRequiredService<LibraryMaintenanceCoordinator>();
        var scanResult = await coordinator.RunAsync();

        if (!scanResult.Success)
        {
            Console.WriteLine($"   ⚠️ Scan encountered an error: {scanResult.ErrorMessage}");
        }
        else
        {
            Console.WriteLine(
                $"   ✅ Scan complete. Created {scanResult.ArtistsCreated} artist(s), {scanResult.ReleasesCreated} release(s)"
            );
        }

        // Reload cache to include any new JSON files created by the scan
        Console.WriteLine("📀 Reloading music library cache after scan...");
        await cache.UpdateCacheAsync();

        var statsAfter = await cache.GetCacheStatisticsAsync();
        Console.WriteLine(
            $"   📊 Post-scan statistics: {statsAfter.ArtistCount} artists, {statsAfter.ReleaseCount} releases, {statsAfter.TrackCount} tracks"
        );

        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine("🚀 MUSIC LIBRARY CACHE READY");
        Console.WriteLine("═══════════════════════════════════════════════");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed to initialize music library cache: {ex.Message}");
        Console.WriteLine("   ⚠️  Application will continue, but cache will be empty");
        Console.WriteLine("═══════════════════════════════════════════════");
    }
}

app.UseWebSockets();

app.MapGraphQL();

// Map attribute-routed controllers (serves /library/* endpoints)
app.MapControllers();

app.MapPost("/test-cors", () => Results.Ok("ok"));

app.Run();
//app.RunWithGraphQLCommands(args);
