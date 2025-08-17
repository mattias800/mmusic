using Google.Apis.Services;
using Hqub.Lastfm;
using Hqub.MusicBrainz;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicGQL.Db.Postgres;
using MusicGQL.EventProcessor;
using MusicGQL.Features.Artists;
using MusicGQL.Features.ArtistImportQueue;
using MusicGQL.Features.ArtistImportQueue.Mutations;
using MusicGQL.Features.ArtistImportQueue.Services;
using MusicGQL.Features.Authentication.Handlers;
using MusicGQL.Features.Authorization;
using MusicGQL.Features.Downloads.Mutations;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.External.Downloads.Prowlarr.Configuration;
using MusicGQL.Features.External.Downloads.QBittorrent.Configuration;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;
using MusicGQL.Features.External.Downloads.Sabnzbd;
using MusicGQL.Features.External.SoulSeek;
using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Features.FileSystem;
using MusicGQL.Features.FileSystem.Mutations;
using MusicGQL.Features.Import;
using MusicGQL.Features.Import.Mutations;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.Likes.Commands;
using MusicGQL.Features.Likes.Events;
using MusicGQL.Features.Likes.Mutations;
using MusicGQL.Features.MusicBrainz.ReleaseGroup;
using MusicGQL.Features.Playlists.Aggregate;
using MusicGQL.Features.Playlists.Commands;
using MusicGQL.Features.Playlists.Import.Spotify;
using MusicGQL.Features.Playlists.Import.Spotify.Mutations;
using MusicGQL.Features.Playlists.Mutations;
using MusicGQL.Features.Playlists.Subscription;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Mutation;
using MusicGQL.Features.ServerLibrary.Reader;
using MusicGQL.Features.ServerLibrary.Subscription;
using MusicGQL.Features.ServerSettings.Commands;
using MusicGQL.Features.ServerSettings.Events;
using MusicGQL.Features.ServerSettings.Mutations;
using MusicGQL.Features.Users.Aggregate;
using MusicGQL.Features.Users.Handlers;
using MusicGQL.Features.Users.Mutations;
using MusicGQL.Features.Users.Services;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Spotify;
using MusicGQL.Integration.Spotify.Configuration;
using MusicGQL.Integration.Youtube.Configuration;
using MusicGQL.Integration.ListenBrainz;
using MusicGQL.Features.ListenBrainz;
using MusicGQL.Features.Import.Services.TopTracks;
using MusicGQL.Types;
using MetaBrainz.ListenBrainz;
using Soulseek;
using Soulseek.Diagnostics;
using SpotifyAPI.Web;
using StackExchange.Redis;
using YouTubeService = MusicGQL.Integration.Youtube.YouTubeService;

var builder = WebApplication.CreateBuilder(args);

// Reduce EF Core SQL logging noise
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

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

// Configure ListenBrainz options
builder.Services.Configure<ListenBrainzConfiguration>(
    builder.Configuration.GetSection(ListenBrainzConfiguration.SectionName)
);

builder.Services.Configure<SoulSeekConnectOptions>(builder.Configuration.GetSection("SoulSeek"));
builder.Services.Configure<ProwlarrOptions>(builder.Configuration.GetSection(ProwlarrOptions.SectionName));
builder.Services.Configure<SabnzbdOptions>(builder.Configuration.GetSection(SabnzbdOptions.SectionName));
builder.Services.Configure<SabnzbdHistoryScannerOptions>(builder.Configuration.GetSection("Sabnzbd:HistoryScanner"));
builder.Services.Configure<QBittorrentOptions>(builder.Configuration.GetSection(QBittorrentOptions.SectionName));

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
    .AddSingleton<SoulSeekUserDiscoveryService>()
    .AddSingleton<SoulSeekReleaseDownloader>()
    .AddScoped<StartDownloadReleaseService>()
    .AddSingleton<DownloadSlotManager>()
    .AddSingleton<IDownloadSlotManager>(sp => sp.GetRequiredService<DownloadSlotManager>())
    .AddSingleton<DownloadQueueService>()
    .AddSingleton<CurrentDownloadStateService>()
    .AddSingleton<DownloadHistoryService>()
    .AddHostedService(sp => sp.GetRequiredService<DownloadSlotManager>())
    // .AddHostedService<DownloadWorker>() // Disabled - now using DownloadSlotManager
    .AddHostedService<MissingLibraryItemsDetectorWorker>()
    .AddSingleton<MusicBrainzService>()
    .AddSingleton<YouTubeService>()
    .AddSingleton<SpotifyService>()
    .AddSingleton<ListenBrainzService>()
    .AddSingleton<UserListenBrainzService>()
    .AddSingleton<ListenBrainzPopularityClient>()
    .AddSingleton<ArtistImportQueueService>()
    .AddSingleton<CurrentArtistImportStateService>()
    .AddSingleton<ImportHistoryService>()
    .AddSingleton<ArtistImportBackgroundQueueService>()
    .AddHostedService<ArtistImportBackgroundService>()
    .AddSingleton<ServerLibraryJsonReader>()
    .AddSingleton<ServerLibraryAssetReader>()
    .AddSingleton<ServerLibraryFileSystemScanner>()
    .AddSingleton<ServerLibraryCache>()
    .AddScoped<MusicGQL.Features.ServerLibrary.MediaFileAssignmentService>()
    .AddScoped<MusicBrainzImportService>()
    .AddScoped<SpotifyImportService>()
    .AddScoped<FanArtDownloadService>()
    .AddScoped<CoverArtDownloadService>()
    .AddScoped<IFolderIdentityService, FolderIdentityService>()
    .AddScoped<IImportExecutor, MusicBrainzImportExecutor>()
    .AddScoped<LibraryMaintenanceCoordinator>()
    .AddScoped<LastFmEnrichmentService>()
    .AddScoped<ReleaseJsonBuilder>()
    .AddScoped<LibraryReleaseImportService>()
    .AddScoped<LibraryImportService>()
    .AddScoped<MusicGQL.Features.ServerLibrary.Share.ArtistShareManifestService>()
    .AddSingleton<MusicGQL.Features.ServerSettings.ServerSettingsAccessor>()
    .AddScoped<MusicGQL.Features.ServerLibrary.Writer.ServerLibraryJsonWriter>()
    .AddSingleton<MusicGQL.Features.ServerSettings.LibraryManifestService>()
    .AddScoped<LikeSongHandler>()
    .AddScoped<UnlikeSongHandler>()
    .AddScoped<UpdateLibraryPathHandler>()
    .AddScoped<UpdateDownloadPathHandler>()
    .AddScoped<UpdateDownloadSlotCountHandler>()
    // Top Tracks Services
    .AddScoped<TopTracksServiceManager>()
    .AddScoped<TopTracksListenBrainzImporter>()
    .AddScoped<TopTracksSpotifyImporter>()
    .AddScoped<TopTracksLastFmImporter>()
    // Download providers
    .AddSingleton<MusicGQL.Features.External.Downloads.SoulSeekDownloadProvider>()
    .AddHttpClient<MusicGQL.Features.External.Downloads.Prowlarr.ProwlarrClient>((serviceProvider, client) =>
    {
        // Configure timeout for Prowlarr requests using options
        var options = serviceProvider.GetRequiredService<IOptions<ProwlarrOptions>>();
        client.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
        
        // Additional configuration for better reliability
        client.DefaultRequestHeaders.Add("User-Agent", "MusicGQL/1.0");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .Services
    .AddHttpClient<SabnzbdClient>(client =>
    {
        // Configure timeout for SABnzbd requests - 30 seconds should be sufficient for local network
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .Services
    .AddHttpClient<MusicGQL.Features.External.Downloads.QBittorrent.QBittorrentClient>(client =>
    {
        // Configure timeout for QBittorrent requests - 30 seconds should be sufficient for local network
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .Services
    .AddSingleton<SabnzbdFinalizeService>()
    .AddSingleton<MusicGQL.Features.External.Downloads.Prowlarr.ProwlarrDownloadProvider>()
    .AddSingleton<MusicGQL.Features.External.Downloads.DownloadProviderCatalog>(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILogger<MusicGQL.Features.External.Downloads.DownloadProviderCatalog>>();
        bool skipSoulSeek = false;
        bool preferProwlarrFirst = false;
        try
        {
            skipSoulSeek = configuration.GetValue<bool>("Download:SkipSoulSeek");
        }
        catch
        {
        }

        try
        {
            preferProwlarrFirst = configuration.GetValue<bool>("Download:PreferProwlarrFirst");
        }
        catch
        {
        }

        var soulSeekProvider = sp.GetRequiredService<MusicGQL.Features.External.Downloads.SoulSeekDownloadProvider>();
        var prowlarrProvider =
            sp.GetRequiredService<MusicGQL.Features.External.Downloads.Prowlarr.ProwlarrDownloadProvider>();

        var providers = new List<MusicGQL.Features.External.Downloads.IDownloadProvider>();

        // Always try Prowlarr first, then Soulseek
        providers.Add(prowlarrProvider);
        if (!skipSoulSeek) providers.Add(soulSeekProvider);

        // Note: preferProwlarrFirst is now always true by default

        try
        {
            logger.LogInformation("[DownloadProviders] skipSoulSeek={Skip}, preferProwlarrFirst={Prefer}", skipSoulSeek,
                preferProwlarrFirst);
            logger.LogInformation("[DownloadProviders] Order: {Order}",
                string.Join(", ", providers.Select(p => p.GetType().Name)));
        }
        catch
        {
        }

        return new MusicGQL.Features.External.Downloads.DownloadProviderCatalog(providers);
    })
    .AddHostedService<SabnzbdWatcherWorker>()
    .AddHostedService<SabnzbdHistoryScannerWorker>()
    // Event processors
    .AddScoped<LikedSongsEventProcessor>()
    .AddScoped<UserEventProcessor>()
    .AddScoped<PlaylistsEventProcessor>()
    .AddScoped<MusicGQL.Features.PlayCounts.PlayCountsEventProcessor>()
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

// Add Spotify Client with automatic client credentials auth
builder.Services.AddSingleton<SpotifyClient>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<SpotifyClientOptions>>().Value;
    if (string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.ClientSecret))
    {
        throw new InvalidOperationException("Spotify ClientId or ClientSecret is not configured.");
    }

    var spotifyConfig = SpotifyClientConfig
        .CreateDefault()
        .WithAuthenticator(
            new ClientCredentialsAuthenticator(options.ClientId!, options.ClientSecret!)
        );

    return new SpotifyClient(spotifyConfig);
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
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MusicGQL.Features.Assets.ExternalAssetStorage>();
builder.Services.AddSingleton<DownloadCancellationService>();
builder.Services.AddSingleton<MusicGQL.Features.ServerLibrary.Services.ArtistDeletionService>();

// Add MVC controllers for asset endpoints
builder.Services.AddControllers();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Options for missing library detector
builder.Services.Configure<MissingLibraryItemsDetectorOptions>(
    builder.Configuration.GetSection("MissingLibraryDetector"));

builder
    .AddGraphQL()
    .AddAuthorization()
    .ModifyRequestOptions(o => { o.ExecutionTimeout = TimeSpan.FromSeconds(60); })
    .AddRedisSubscriptions(_ =>
        ConnectionMultiplexer.Connect(
            builder.Configuration.GetConnectionString("Redis") ?? string.Empty
        )
    )
    .AddDiagnosticEventListener<MyExecutionEventListener>()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddTypeExtension<SpotifyPlaylistSearchRoot>()
    .AddType<SpotifyTrack>()
    .AddTypeExtension<ImportSpotifyPlaylistMutation>()
    .AddType<ImportSpotifyPlaylistSuccess>()
    .AddType<ImportSpotifyPlaylistError>()
    .AddTypeExtension<ImportSpotifyPlaylistArtistsMutation>()
    .AddType<ImportArtistsFromSpotifyPlaylistSuccess>()
    .AddType<ImportArtistsFromSpotifyPlaylistError>()
    .AddTypeExtension<SoulSeekSubscription>()
    .AddTypeExtension<LibrarySubscription>()
    .AddTypeExtension<ImportSubscription>()
    .AddTypeExtension<ArtistImportSearchRoot>()
    .AddTypeExtension<ArtistImportMutations>()
    .AddTypeExtension<ListenBrainzQueryRoot>()
    .AddTypeExtension<ListenBrainzMutations>()
    .AddType<SubmitListenSuccess>()
    .AddType<SubmitListenError>()
    .AddTypeExtension<UpdateListenBrainzSettingsMutation>()
    .AddType<UpdateListenBrainzSettingsSuccess>()
    .AddType<UpdateListenBrainzSettingsError>()
    .AddTypeExtension<UpdateTopTracksServiceSettingsMutation>()
    .AddType<UpdateTopTracksServiceSettingsSuccess>()
    .AddType<UpdateTopTracksServiceSettingsError>()
    .AddTypeExtension<DownloadsSearchRoot>()
    .AddTypeExtension<DownloadsSubscription>()
    .AddTypeExtension<DownloadQueueMutations>()
    .AddTypeExtension<PlaylistSubscription>()
    .AddTypeExtension<SetPlaylistItemArtistMusicBrainzMatchMutation>()
    .AddType<SetPlaylistItemArtistMusicBrainzMatchSuccess>()
    .AddType<SetPlaylistItemArtistMusicBrainzMatchNotFound>()
    .AddType<SetPlaylistItemArtistMusicBrainzMatchError>()
    .AddTypeExtension<SetPlaylistItemArtistMatchMutation>()
    .AddType<SetPlaylistItemArtistMatchSuccess>()
    .AddType<SetPlaylistItemArtistMatchNotFound>()
    .AddTypeExtension<StartDownloadReleaseMutation>()
    .AddType<StartDownloadReleaseSuccess>()
    .AddType<StartDownloadReleaseUnknownError>()
    .AddType<StartDownloadReleaseAccepted>()
    .AddTypeExtension<StartBulkDownloadForArtistMutation>()
    .AddType<StartBulkDownloadForArtistSuccess>()
    .AddType<StartBulkDownloadForArtistError>()
    .AddTypeExtension<UnlikeSongMutation>()
    .AddType<UnlikeSongSuccess>()
    .AddType<UnlikeSongAlreadyNotLiked>()
    .AddTypeExtension<LikeSongMutation>()
    .AddTypeExtension<ImportArtistMutation>()
    .AddType<IArtistBase>()
    .AddType<ImportArtistSuccess>()
    .AddType<ImportArtistError>()
    .AddType<LikeSongSuccess>()
    .AddType<LikeSongAlreadyLiked>()
    .AddType<LikeSongSongDoesNotExist>()
    .AddTypeExtension<UpdateLibraryPathMutation>()
    .AddType<UpdateLibraryPathSuccess>()
    .AddTypeExtension<CreateLibraryManifestMutation>()
    .AddType<CreateLibraryManifestSuccess>()
    .AddType<CreateLibraryManifestError>()
    .AddTypeExtension<UpdateDownloadPathMutation>()
    .AddType<UpdateDownloadPathSuccess>()
    .AddTypeExtension<UpdateDownloadSlotCountMutation>()
    .AddType<UpdateDownloadSlotCountSuccess>()
    .AddType<UpdateDownloadSlotCountError>()
    .AddTypeExtension<UpdateSoulSeekSearchTimeLimitMutation>()
    .AddType<UpdateSoulSeekSearchTimeLimitSuccess>()
    .AddType<UpdateSoulSeekSearchTimeLimitError>()
    .AddTypeExtension<UpdateSoulSeekNoDataTimeoutMutation>()
    .AddType<UpdateSoulSeekNoDataTimeoutSuccess>()
    .AddType<UpdateSoulSeekNoDataTimeoutError>()
    .AddTypeExtension<ScanLibraryForMissingJsonMutation>()
    .AddType<ScanLibraryForMissingJsonSuccess>()
    .AddTypeExtension<RefreshArtistMetaDataMutation>()
    .AddTypeExtension<GenerateArtistShareFilesMutation>()
    .AddType<GenerateArtistShareFilesSuccess>()
    .AddType<GenerateArtistShareFilesError>()
    .AddType<RefreshArtistMetaDataSuccess>()
    .AddType<RefreshArtistMetaDataError>()
    .AddTypeExtension<RefreshReleaseMutation>()
    .AddType<RefreshReleaseSuccess>()
    .AddType<RefreshReleaseError>()
    .AddTypeExtension<RefreshAllReleasesForArtistMutation>()
    .AddType<RefreshAllReleasesForArtistSuccess>()
    .AddType<RefreshAllReleasesForArtistError>()
    .AddTypeExtension<ScanReleaseFolderForMediaMutation>()
    .AddType<ScanReleaseFolderForMediaSuccess>()
    .AddType<ScanReleaseFolderForMediaError>()
    .AddTypeExtension<RedownloadReleaseMutation>()
    .AddType<RedownloadReleaseSuccess>()
    .AddType<RedownloadReleaseError>()
    .AddTypeExtension<RefreshArtistTopTracksMutation>()
    .AddType<RefreshArtistTopTracksSuccess>()
    .AddType<RefreshArtistTopTracksUnknownError>()
    .AddTypeExtension<CreateUserMutation>()
    .AddType<CreateUserSuccess>()
    .AddType<CreateUserError>()
    .AddTypeExtension<UpdateUserListenBrainzCredentialsMutation>()
    .AddType<UpdateUserListenBrainzCredentialsSuccess>()
    .AddType<UpdateUserListenBrainzCredentialsError>()
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
    .AddTypeExtension<AddTrackToPlaylistMutation>()
    .AddType<AddTrackToPlaylistSuccess>()
    .AddType<AddTrackToPlaylistError>()
    .AddTypeExtension<RemoveItemFromPlaylistMutation>()
    .AddType<RemoveItemFromPlaylistSuccess>()
    .AddType<RemoveItemFromPlaylistError>()
    .AddTypeExtension<MovePlaylistItemMutation>()
    .AddType<MovePlaylistItemSuccess>()
    .AddType<MovePlaylistItemError>()
    .AddTypeExtension<DeleteReleaseAudioMutation>()
    .AddType<DeleteReleaseAudioSuccess>()
    .AddType<DeleteReleaseAudioError>()
    
    .AddTypeExtension<DeleteArtistMutation>()
    .AddType<DeleteArtistSuccess>()
    .AddType<DeleteArtistError>()
    .AddTypeExtension<SetArtistMatchMutation>()
    .AddType<SetArtistMusicBrainzMatchSuccess>()
    .AddType<SetArtistMusicBrainzMatchError>()
    .AddType<SetArtistSpotifyMatchSuccess>()
    .AddType<SetArtistSpotifyMatchError>()
    .AddTypeExtension<SetReleaseGroupMutation>()
    .AddType<SetReleaseGroupSuccess>()
    .AddType<SetReleaseGroupError>()
    // Release match override + MB release scoring
    .AddTypeExtension<SetReleaseMatchOverrideMutation>()
    .AddType<SetReleaseMatchOverrideSuccess>()
    .AddType<SetReleaseMatchOverrideError>()
    .AddTypeExtension<FixReleaseTypeMutation>()
    .AddType<FixReleaseTypeSuccess>()
    .AddType<FixReleaseTypeError>()
    .AddTypeExtension<ReleasesWithScoresQuery>()
    .AddTypeExtension<FileSystemSearchRoot>()
    .AddTypeExtension<CreateDirectoryMutation>()
    .AddType<CreateDirectorySuccess>()
    .AddType<CreateDirectoryError>();

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

// Configure ListenBrainz client
builder.Services.AddSingleton<ListenBrainz>(provider =>
{
    var contact = new Uri("https://github.com/yourusername/mmusic");
    // ListenBrainz v4.0.0 expects (application, version, contact)
    return new ListenBrainz("MusicGQL", "1.0", contact);
});

builder.Services.AddFanArtTVClient(options =>
{
    var fanartOptions = builder.Configuration.GetSection("Fanart").Get<FanartOptions>();
    options.ApiKey = fanartOptions?.ApiKey;
    options.BaseAddress = fanartOptions?.BaseAddress;
});

builder.Services.AddHostedService<ScheduledTaskPublisher>();
builder.Services.AddHostedService<LibraryImportWorker>();

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
    _ = soulSeekService.Connect();

    // 🎵 Initialize and populate the music library cache
    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════");
    Console.WriteLine("🎵 INITIALIZING MUSIC LIBRARY CACHE");
    Console.WriteLine("═══════════════════════════════════════════════");

    var cache = scope.ServiceProvider.GetRequiredService<ServerLibraryCache>();

    try
    {
        Console.WriteLine("📀 Loading music library from disk...");
        try
        {
            var serverSettingsAccessor = scope.ServiceProvider
                .GetRequiredService<MusicGQL.Features.ServerSettings.ServerSettingsAccessor>();
            var settings = await serverSettingsAccessor.GetAsync();
            var lib = string.IsNullOrWhiteSpace(settings.LibraryPath) ? "(not set)" : settings.LibraryPath;
            Console.WriteLine($"   🔍 Library path: {lib}");
        }
        catch
        {
            Console.WriteLine("   🔍 Library path: (error reading settings)");
        }

        await cache.UpdateCacheAsync();

        var stats = await cache.GetCacheStatisticsAsync();
        Console.WriteLine($"✅ Cache loaded successfully!");
        Console.WriteLine(
            $"   📊 Statistics: {stats.ArtistCount} artists, {stats.ReleaseCount} releases, {stats.TrackCount} tracks"
        );
        Console.WriteLine($"   🕒 Last updated: {stats.LastUpdated:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine();

        // Only show detailed listing if explicitly requested (reduce startup noise)
        var showDetailedListing = Environment.GetEnvironmentVariable("MUSICGQL_SHOW_DETAILED_STARTUP") == "true";
        
        if (showDetailedListing)
        {
            // Display all artists and their albums (verbose mode)
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
        }
        else
        {
            // Show only summary information (default mode - less noisy)
            var artists = await cache.GetAllArtistsAsync();
            if (artists.Count > 0)
            {
                var sampleArtists = artists.Take(5).OrderBy(a => a.Name).ToList();
                Console.WriteLine($"🎤 Library contains {artists.Count} artists");
                if (artists.Count > 5)
                {
                    Console.WriteLine($"   📋 Sample artists: {string.Join(", ", sampleArtists.Select(a => a.Name))}...");
                }
                else
                {
                    Console.WriteLine($"   📋 Artists: {string.Join(", ", sampleArtists.Select(a => a.Name))}");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("📭 No artists found in library");
                Console.WriteLine("   💡 Make sure your library folder contains artist.json files");
            }
            
            // Add helpful hint about detailed logging
            Console.WriteLine("💡 To see detailed artist and release listing during startup, set:");
            Console.WriteLine("   MUSICGQL_SHOW_DETAILED_STARTUP=true");
            Console.WriteLine();
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