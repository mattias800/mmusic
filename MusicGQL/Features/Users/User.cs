using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Likes;
using MusicGQL.Features.Playlists;
using MusicGQL.Features.Users.Db;
using MusicGQL.Features.Users.Roles;
using MusicGQL.Features.Authorization;

namespace MusicGQL.Features.Users;

public record User([property: GraphQLIgnore] DbUser Model)
{
    [ID]
    public string Id() => Model.UserId.ToString();

    public string Username() => Model.Username;

    public DateTime CreatedAt() => Model.CreatedAt;

    public DateTime UpdatedAt() => Model.UpdatedAt;

    public string? ListenBrainzUserId() => Model.ListenBrainzUserId;

    public bool HasListenBrainzToken() => !string.IsNullOrEmpty(Model.ListenBrainzToken);

    [GraphQLName("roles")]
    public int GetRoles() => (int)Model.Roles;

    public bool IsAdmin([Service] UserRoleAuthorizer auth) => auth.IsAdmin(Model.Roles);
    public bool CanCreatePlaylists([Service] UserRoleAuthorizer auth) => auth.CanCreatePlaylists(Model.Roles);
    public bool CanTriggerDownloads([Service] UserRoleAuthorizer auth) => auth.CanTriggerDownloads(Model.Roles);
    public bool CanManageUserRoles([Service] UserRoleAuthorizer auth) => auth.CanManageUserRoles(Model.Roles);
    public bool CanViewDownloads([Service] UserRoleAuthorizer auth) => auth.CanViewDownloads(Model.Roles);
    public bool CanEditExternalAuth([Service] UserRoleAuthorizer auth) => auth.CanEditExternalAuth(Model.Roles);

    public async Task<IEnumerable<LikedSong>> LikedSongs([Service] EventDbContext dbContext)
    {
        var likedSongs = await dbContext
            .LikedSongs.Where(ls => ls.LikedByUserId == Model.UserId)
            .Select(ls => new LikedSong(ls.RecordingId))
            .ToListAsync();

        return likedSongs;
    }

    public async Task<IEnumerable<Playlist>> Playlists([Service] EventDbContext dbContext)
    {
        var playlists = await dbContext
            .Playlists.Where(p => p.UserId == Model.UserId)
            .ToListAsync();

        return playlists.Select(p => new Playlist(p));
    }
}
