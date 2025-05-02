using Hqub.MusicBrainz;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Aggregates;
using MusicGQL.Db;
using MusicGQL.Features.User.Mutations;
using MusicGQL.Types;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<MusicBrainzClient>()
    .AddScoped<EventProcessor>();

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseSqlite("Data Source=events.db"));

builder
    .AddGraphQL()
    .AddDiagnosticEventListener<MyExecutionEventListener>()
    .AddMutationConventions(applyToAllMutations: true)
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<LikeSongMutation>()
    ;

var app = builder.Build();

// 🟢 Run event processor once on startup
using (var scope = app.Services.CreateScope())
{
    var processor = scope.ServiceProvider.GetRequiredService<EventProcessor>();
    await processor.ProcessEvents();
}

app.MapGraphQL();

app.RunWithGraphQLCommands(args);