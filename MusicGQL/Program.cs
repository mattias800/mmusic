using Hqub.MusicBrainz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MusicBrainzClient>();

builder
    .AddGraphQL()
    .AddDiagnosticEventListener<MyExecutionEventListener>()
    .AddTypes();

var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);