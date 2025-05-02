using Hqub.MusicBrainz;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Aggregates;
using MusicGQL.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<MusicBrainzClient>()
    .AddScoped<EventProcessor>();

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseSqlite("Data Source=events.db"));

builder
    .AddGraphQL()
    .AddDiagnosticEventListener<MyExecutionEventListener>()
    .AddTypes();

var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);