using Hqub.Lastfm;
using Hqub.MusicBrainz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicGQL.Aggregates;
using MusicGQL.Db;
using MusicGQL.Features.LikedSongs.Commands;
using MusicGQL.Features.LikedSongs.Mutations;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Types;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddHybridCache()
    .Services
    .AddSingleton<IMemoryCache, MemoryCache>()
    .AddSingleton<MusicBrainzService>()
    .AddScoped<LikeSongHandler>()
    .AddScoped<UnlikeSongHandler>()
    .AddScoped<EventProcessor>();

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseSqlite("Data Source=events.db")
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("RedisConnectionString");
});

builder
    .AddGraphQL()
    .AddDiagnosticEventListener<MyExecutionEventListener>()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<LikeSongMutation>()
    .AddTypeExtension<UnlikeSongMutation>()
    .AddType<LikedSongSuccess>()
    .AddType<LikedSongAlreadyLiked>()
    .AddType<LikedSongSongDoesNotExist>();

builder.Services.Configure<LastfmOptions>(
    builder.Configuration.GetSection("Lastfm"));

builder.Services.AddSingleton<LastfmClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<LastfmOptions>>().Value;
    return new LastfmClient(options.ApiKey);
});

builder.Services.AddHttpClient<MusicBrainzClient>(client =>
{
    client.BaseAddress = new Uri("https://musicbrainz.org/ws/2/");
    client.DefaultRequestHeaders.Add("User-Agent", "Hqub.MusicBrainz/3.0 (https://github.com/avatar29A/MusicBrainz)");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddFanArtTVClient(options =>
{
    var fanartOptions = builder.Configuration.GetSection("Fanart").Get<FanartOptions>();
    options.ApiKey = fanartOptions?.ApiKey;
    options.BaseAddress = fanartOptions?.BaseAddress;
});

var app = builder.Build();

// 🟢 Run event processor once on startup
using (var scope = app.Services.CreateScope())
{
    var processor = scope.ServiceProvider.GetRequiredService<EventProcessor>();
    await processor.ProcessEvents();
}

app.MapGraphQL();

app.RunWithGraphQLCommands(args);