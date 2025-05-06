using Hqub.Lastfm;
using Hqub.MusicBrainz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicGQL.Aggregates;
using MusicGQL.Db;
using MusicGQL.Features.Downloads.Mutations;
using MusicGQL.Features.LikedSongs.Commands;
using MusicGQL.Features.LikedSongs.Mutations;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Sagas.DownloadRelease;
using MusicGQL.Types;
using Rebus.Config;
using Rebus.Routing.TypeBased;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddHybridCache()
    .Services.AddSingleton<IMemoryCache, MemoryCache>()
    .AddSingleton<MusicBrainzService>()
    .AddScoped<LikeSongHandler>()
    .AddScoped<UnlikeSongHandler>()
    .AddScoped<EventProcessor>();

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder
    .AddGraphQL()
    .AddDiagnosticEventListener<MyExecutionEventListener>()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<StartDownloadReleaseMutation>()
    .AddType<StartDownloadReleaseSuccess>()
    .AddTypeExtension<LikeSongMutation>()
    .AddTypeExtension<UnlikeSongMutation>()
    .AddType<LikeSongSuccess>()
    .AddType<LikeSongAlreadyLiked>()
    .AddType<LikeSongSongDoesNotExist>();

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

builder.Services.AddRebus(rebus =>
        rebus
            .Routing(r => r.TypeBased().MapAssemblyOf<ApplicationAssemblyReference>("mmusic-queue"))
            .Transport(t =>
                t.UseRabbitMq(
                    builder.Configuration.GetConnectionString("MessageBroker"),
                    "mmusic-queue"
                )
            )
            .Sagas(s =>
                s.StoreInPostgres(
                    builder.Configuration.GetConnectionString("Postgres"),
                    "sagas",
                    "saga_indexes"
                )
            ), onCreated: async bus =>
    {
        await bus.Subscribe<FoundReleaseInMusicBrainz>();
        await bus.Subscribe<FoundReleaseDownload>();
    }
);

builder.Services.AutoRegisterHandlersFromAssemblyOf<ApplicationAssemblyReference>();

var app = builder.Build();

// 🟢 Run event processor once on startup
using (var scope = app.Services.CreateScope())
{
    var processor = scope.ServiceProvider.GetRequiredService<EventProcessor>();
    await processor.ProcessEvents();
}

app.MapGraphQL();

app.RunWithGraphQLCommands(args);