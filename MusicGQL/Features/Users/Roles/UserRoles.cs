namespace MusicGQL.Features.Users.Roles;

[Flags]
public enum UserRoles
{
    None = 0,
    Admin = 1 << 0,
    CreatePlaylists = 1 << 1,
    TriggerDownloads = 1 << 2,
    ManageUserRoles = 1 << 3,
    ViewDownloads = 1 << 4,
    EditExternalAuth = 1 << 5,
}

public static class UserRolesExtensions
{
    public static bool HasAnyRole(this UserRoles roles, params UserRoles[] required)
    {
        foreach (var r in required)
        {
            if ((roles & r) == r)
                return true;
        }
        return false;
    }
}
