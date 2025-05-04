using Hqub.MusicBrainz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MusicGQL.Aggregates;
using MusicGQL.Db;
using MusicGQL.Features.LikedSongs.Commands;
using MusicGQL.Features.LikedSongs.Mutations;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Types;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddMemoryCache()
    .AddSingleton<IMemoryCache, MemoryCache>()
    .AddSingleton<MusicBrainzClient>()
    .AddSingleton<MusicBrainzService>()
    .AddScoped<LikeSongHandler>()
    .AddScoped<UnlikeSongHandler>()
    .AddScoped<EventProcessor>();

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseSqlite("Data Source=events.db")
);

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

var app = builder.Build();

// 🟢 Run event processor once on startup
using (var scope = app.Services.CreateScope())
{
    var processor = scope.ServiceProvider.GetRequiredService<EventProcessor>();
    await processor.ProcessEvents();
}

app.MapGraphQL();

app.RunWithGraphQLCommands(args);
