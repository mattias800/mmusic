using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Features.ServerLibrary.Common.Db;
using MusicGQL.Features.ServerLibrary.Recording.Db;
using MusicGQL.Features.ServerLibrary.Release.Db;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;
using Neo4j.Driver;
using INode = Neo4j.Driver.INode;

namespace MusicGQL.Integration.Neo4j;

public static class Neo4jNodeMapperExtensions
{
    public static DbArtist ToDbArtist(this INode node)
    {
        return new DbArtist
        {
            Id = node["Id"].As<string>() ?? string.Empty,
            Name = node["Name"].As<string>() ?? string.Empty,
            SortName = node["SortName"].As<string>() ?? string.Empty,
            Gender = node.Properties.GetValueOrDefault("Gender") as string,
        };
    }

    public static DbNameCredit ToDbNamedCredit(this IRelationship r)
    {
        return new DbNameCredit
        {
            JoinPhrase = r["JoinPhrase"].As<string>() ?? string.Empty,
            Name = r["Name"].As<string>() ?? string.Empty,
        };
    }

    public static DbRelation ToDbRelation(this INode node)
    {
        return new DbRelation
        {
            Begin = node["Begin"].As<string>() ?? string.Empty,
            End = node["End"].As<string>() ?? string.Empty,
            Ended = node["Ended"].As<bool>(),
            Attributes = node["Attributes"].As<string[]>(),
            Url = node["Url"].As<string>() ?? string.Empty,
        };
    }

    public static DbRecording ToDbRecording(this INode node)
    {
        return new DbRecording
        {
            Id = node["Id"].As<string>() ?? string.Empty,
            Title = node["Title"].As<string>() ?? string.Empty,
            Length = node["Length"]?.As<int?>(),
            Disambiguation = node["Disambiguation"]?.As<string>(),
        };
    }

    public static DbRelease ToDbRelease(this INode node)
    {
        return new DbRelease
        {
            Id = node["Id"].As<string>() ?? string.Empty,
            Title = node["Title"].As<string>() ?? string.Empty,
            Date = node["Date"]?.As<string>(),
            Status = node["Status"]?.As<string>(),
        };
    }

    public static DbReleaseGroup ToDbReleaseGroup(this INode node)
    {
        return new DbReleaseGroup
        {
            Id = node["Id"].As<string>() ?? string.Empty,
            Title = node["Title"].As<string>() ?? string.Empty,
            PrimaryType = node["PrimaryType"].As<string>() ?? string.Empty,
            SecondaryTypes =
                node["SecondaryTypes"]?.As<List<object>>()?.Select(s => s.ToString()).ToList()
                ?? new List<string>(),
            FirstReleaseDate = node["FirstReleaseDate"].As<string>() ?? string.Empty,
        };
    }

    public static DbLabel ToDbLabel(this INode node)
    {
        return new DbLabel
        {
            Id = node["Id"].As<string>() ?? string.Empty,
            Disambiguation = node["Disambiguation"].As<string>() ?? string.Empty,
            Name = node["Name"]?.As<string>() ?? string.Empty,
        };
    }
}
