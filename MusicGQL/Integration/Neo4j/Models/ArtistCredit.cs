using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Features.ServerLibrary.Common.Db;

namespace MusicGQL.Integration.Neo4j.Models;

public record ArtistCredit(DbNameCredit NameCredit, DbArtist Artist);
