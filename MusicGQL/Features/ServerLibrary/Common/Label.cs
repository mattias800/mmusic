using MusicGQL.Features.ServerLibrary.Common.Db;

namespace MusicGQL.Features.ServerLibrary.Common;

public record Label([property: GraphQLIgnore] DbLabel Model)
{
    public string Id() => Model.Id;

    public string Name() => Model.Name;

    public string Disambiguation() => Model.Disambiguation;

    // public List<Alias> Aliases => Model.Aliases;
}
